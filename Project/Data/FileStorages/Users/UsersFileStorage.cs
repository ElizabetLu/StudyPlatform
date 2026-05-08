using StudyPlatform.Data.FileStorages.Base;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.Users
{
    public class UsersFileStorage
    {
        // to return system error messages.

        public class GetAllUsersResult : FileStorageResultBase
        {
            public List<User> Users { get; set; } = new List<User>();
        }

        // 1. Create a private storage object that reads and writes Users objects to the Users file.

        private readonly JsonFileStorage<User> _storage = new JsonFileStorage<User>(Constants.FilePaths.Users);

        // 2. Creating a dictionary for fast search by username.

        private readonly Dictionary<string, User> _usernameIndex = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        // 3. Creating a dictionary for fast search by email.

        private readonly Dictionary<string, User> _emailIndex = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
        
        private bool _indexLoaded = false;

        // 4. Checks if the dictionary _index is loaded before using it.

        private async Task EnsureIndexAsync()
        {
            if (!_indexLoaded)

            {
                // gets all users from JSON storage. and clears the old indexes.

                List<User> users = await _storage.ReadAllAsync();

                _usernameIndex.Clear();

                _emailIndex.Clear();

                foreach (User user in users)

                {
                    _usernameIndex[user.Username] = user;

                    _emailIndex[user.Email] = user;
                }

                _indexLoaded = true;
            }
        }

        private void InvalidateIndex()
        {
            _indexLoaded = false;
        }

        // 5. Reads all users and returns them in a form of a list.

        public async Task<GetAllUsersResult> GetAllUsersAsync()
        {
            var result = new GetAllUsersResult();

            result.Users = await _storage.ReadAllAsync();

            return result;
        }

        // 6. Read all users, adds a new one and save the list back to the file.

        public async Task AddUserAsync(User user)
        {
            List<User> users = await _storage.ReadAllAsync();
            users.Add(user);
            await _storage.WriteAllAsync(users);
            InvalidateIndex();
        }



        public async Task<FileStorageResultBase> UpdateUserAsync(User updatedUser)
        {
            List<User> users = await _storage.ReadAllAsync();

            int index = users.FindIndex(u => u.Id == updatedUser.Id);

            // Create result object

            FileStorageResultBase result = new FileStorageResultBase();

            if (index == -1)

            {
                result.IsError = true;

                result.ErrorMessage = "User not found.";
            }

            else

            {
                users[index] = updatedUser; // replace old user

                await _storage.WriteAllAsync(users); // saving updated list

                InvalidateIndex(); // no longer relevant
            }

            return result;
        }

        public async Task DeleteUserAsync(int userId)
        {
            List<User> users = await _storage.ReadAllAsync();

            users.RemoveAll(u => u.Id == userId);

            await _storage.WriteAllAsync(users); // saving updated list

            InvalidateIndex();
        }

        // 7. Find user by email or username.

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            await EnsureIndexAsync();

            // First checking username dictionary, then checking email dictionary.
            // If exists - return user otherwise return null.

            User? result = _usernameIndex.ContainsKey(usernameOrEmail)
                ? _usernameIndex[usernameOrEmail]
                : _emailIndex.ContainsKey(usernameOrEmail) ? _emailIndex[usernameOrEmail] : null;

            return result;
        }

        // 8. Checks if the username already exists.

        public async Task<bool> UsernameExistsAsync(string username)
        {
            await EnsureIndexAsync();

            return _usernameIndex.ContainsKey(username);
        }

        // 9. Checks if the email already exists.

        public async Task<bool> EmailExistsAsync(string email)
        {
            await EnsureIndexAsync();

            return _emailIndex.ContainsKey(email);
        }
    }
}