using StudyPlatform.Data.FileStorages.Base;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.Profiles
{
    public class ProfilesFileStorage
    {
        // 1. Create a private storage object that reads and writes Profiles objects to the Profiles file.

        private readonly JsonFileStorage<Profile> _storage = new JsonFileStorage<Profile>(Constants.FilePaths.Profiles);

        // 2. To make searching by id faster.

        private readonly Dictionary<int, Profile> _index = new Dictionary<int, Profile>();

        private bool _indexLoaded = false;

        // 3. Checks if the dictionary _index is loaded before using it.

        private async Task EnsureIndexAsync()
        {
            if (!_indexLoaded)

            {
                // gets all profiles from JSON storage. and clears the old index.

                List<Profile> profiles = await _storage.ReadAllAsync();
                
                _index.Clear();

                foreach (Profile profile in profiles)

                {
                    _index[profile.UserId] = profile;
                }

                _indexLoaded = true;
            }
        }

        private void InvalidateIndex()
        {
            _indexLoaded = false;
        }

        // 4. Read all profiles, adds a new one and save the list back to the file.

        public async Task AddProfileAsync(Profile profile)
        {
            List<Profile> profiles = await _storage.ReadAllAsync();

            profiles.Add(profile);

            await _storage.WriteAllAsync(profiles);

            InvalidateIndex();
        }

        public async Task<FileStorageResultBase> UpdateProfileAsync(Profile updatedProfile)
        {
            List<Profile> profiles = await _storage.ReadAllAsync();

            int index = profiles.FindIndex(p => p.UserId == updatedProfile.UserId);

            // Create result object

            FileStorageResultBase result = new FileStorageResultBase();

            if (index == -1)

            {
                result.IsError = true;

                result.ErrorMessage = "Profile not found.";
            }

            else

            {
                profiles[index] = updatedProfile;  // replace old profile

                await _storage.WriteAllAsync(profiles); // saving updated list

                InvalidateIndex(); // no longer relevant
            }

            return result;
        }

        public async Task DeleteProfileByUserIdAsync(int userId)
        {
            List<Profile> profiles = await _storage.ReadAllAsync();

            profiles.RemoveAll(p => p.UserId == userId);

            await _storage.WriteAllAsync(profiles); // saving updated list

            InvalidateIndex();
        }

        // 5. If the profile exists, gets the profile.

        public async Task<Profile?> GetProfileByUserIdAsync(int userId)
        {
            await EnsureIndexAsync();

            Profile? profile = _index.ContainsKey(userId) ? _index[userId] : null;

            return profile;
        }
    }
}