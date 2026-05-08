using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Profiles;
using StudyPlatform.Data.FileStorages.Users;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Admin
{
    public class AdminPanel
    {
        private readonly UsersFileStorage _usersStorage;

        private readonly ProfilesFileStorage _profilesStorage;

        // 1. To get all users with their profiles. 

        public class UsersResult : BusinessLogicResultBase
        {
            public List<(User User, Profile? Profile)> UsersWithProfiles { get; set; } = new List<(User, Profile?)>();
        }

        

        public class ActionResult : BusinessLogicResultBase
        {
            public string SuccessMessage { get; set; } = string.Empty;
        }



        public AdminPanel(UsersFileStorage usersStorage, ProfilesFileStorage profilesStorage)
        {
            _usersStorage = usersStorage;
            _profilesStorage = profilesStorage;
        }

        // 2. Checks if the current session is allowed to use admin functions.


        private ActionResult DenyAccess()
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else if (CurrentSession.LoggedInUser!.Role != UserRole.Admin) // if the user is logged-in,
                                                                          // then LoggedInUser should not be null.
            {
                result.IsError = true;

                result.ErrorMessage = "Access denied. Admins only.";
            }

            return result;
        }

        // 3. Returns all users together with their profiles.

        public async Task<UsersResult> GetAllUsersWithProfilesAsync()
        {
            var deny = DenyAccess();

            var result = new UsersResult();

            if (deny.IsError) // no access

            {
                result.IsError = true;

                result.ErrorMessage = deny.ErrorMessage;
            }

            else

            {
                List<User> users = (await _usersStorage.GetAllUsersAsync()).Users
                    .OrderBy(u => u.Id)
                    .ToList();

                List<Profile> allProfiles = new List<Profile>();

                // For each user, load profile

                foreach (User user in users)

                {
                    // get profile by user id

                    Profile? profile = await _profilesStorage.GetProfileByUserIdAsync(user.Id); 
                    allProfiles.Add(profile ?? new Profile { UserId = user.Id }); // if profile = null,
                                                                                  // add a new one with id only
                }

                // joins user's with their profiles by id.

                result.UsersWithProfiles = users
                    .Join(allProfiles,
                        u => u.Id,
                        p => p.UserId,
                        (u, p) => (u, (Profile?)p))
                    .ToList();
            }

            return result;
        }

        public async Task<ActionResult> BlockUserAsync(int targetUserId)
        {
            var deny = DenyAccess();

            ActionResult result;

            if (deny.IsError)
            
            {
                result = deny;
            }

            else
            
            {
                result = await SetBlockedStatusAsync(targetUserId, true, "blocked");
            }

            return result;
        }

        public async Task<ActionResult> UnblockUserAsync(int targetUserId)
        {
            var deny = DenyAccess();

            ActionResult result;

            if (deny.IsError)

            {
                result = deny;
            }

            else

            {
                result = await SetBlockedStatusAsync(targetUserId, false, "unblocked"); 
            }

            return result;
        }


        // 4. For changing user's blocking status.

        private async Task<ActionResult> SetBlockedStatusAsync(int targetUserId, bool blocked, string verb)
        {
            var result = new ActionResult();

            List<User> users = (await _usersStorage.GetAllUsersAsync()).Users; // load all users

            User? user = users.FirstOrDefault(u => u.Id == targetUserId); // find a certain user by id

            if (user == null)

            {
                result.IsError = true;

                result.ErrorMessage = "User not found.";
            }

            else

            {
                user.IsBlocked = blocked; // change the user's blocking status

                var updateResult = await _usersStorage.UpdateUserAsync(user); // save the result

                if (updateResult.IsError) // if update fail

                {
                    result.IsError = true;

                    result.ErrorMessage = updateResult.ErrorMessage;
                }

                else

                {
                    result.SuccessMessage = $"User '{user.Username}' has been {verb}.";
                }
            }

            return result;
        }

        // 5. Delete user

        public async Task<ActionResult> DeleteUserAsync(int targetUserId)
        {
            var deny = DenyAccess();

            var result = new ActionResult();

            if (deny.IsError)

            {
                result.IsError = true;
                result.ErrorMessage = deny.ErrorMessage;
            }

            else

            {
                List<User> users = (await _usersStorage.GetAllUsersAsync()).Users;

                User? user = users.FirstOrDefault(u => u.Id == targetUserId);

                if (user == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "User not found.";
                }

                else

                {
                    await _usersStorage.DeleteUserAsync(targetUserId); // delete user

                    await _profilesStorage.DeleteProfileByUserIdAsync(targetUserId); // delete user's profile

                    result.SuccessMessage = $"User '{user.Username}' and their profile have been deleted.";
                }
            }

            return result;
        }

        // 6. Change user's role.

        public async Task<ActionResult> ChangeUserRoleAsync(int targetUserId, UserRole newRole)
        {
            var deny = DenyAccess();

            var result = new ActionResult();

            if (deny.IsError)

            {
                result.IsError = true;

                result.ErrorMessage = deny.ErrorMessage;
            }

            else

            {
                List<User> users = (await _usersStorage.GetAllUsersAsync()).Users;

                User? user = users.FirstOrDefault(u => u.Id == targetUserId);

                if (user == null)
                
                {
                    result.IsError = true;

                    result.ErrorMessage = "User not found.";
                }

                else

                {
                    user.Role = newRole;

                    var updateResult = await _usersStorage.UpdateUserAsync(user); // saving results

                    if (updateResult.IsError) // if update failed
                    
                    {
                        result.IsError = true;

                        result.ErrorMessage = updateResult.ErrorMessage;
                    }

                    else

                    {
                        result.SuccessMessage = $"User '{user.Username}' role changed to {newRole}.";
                    }
                }
            }

            return result;
        }
    }
}