using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.LoginEvents;
using StudyPlatform.Data.FileStorages.Profiles;
using StudyPlatform.Data.FileStorages.Users;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StudyPlatform.BusinessLogics.Auth
{
    public delegate void AuthNotification(string message);

    public class AuthPanel
    {

        // 1. Create storages by the class

        private readonly UsersFileStorage _usersStorage;

        private readonly ProfilesFileStorage _profilesStorage;

        private readonly LoginEventsFileStorage _loginEventsStorage;

        public class ActionResult : BusinessLogicResultBase
        {
            public string SuccessMessage { get; set; } = string.Empty;
        }

        public class ProfileResult : BusinessLogicResultBase
        {
            public Profile? Profile { get; set; }
        }

        public AuthPanel(UsersFileStorage usersStorage, ProfilesFileStorage profilesStorage,
            LoginEventsFileStorage loginEventsStorage)
        {
            _usersStorage = usersStorage;
            _profilesStorage = profilesStorage;
            _loginEventsStorage = loginEventsStorage;
        }



        public async Task<ActionResult> RegisterAsync(string username, string email, string password)
        {
            var result = new ActionResult(); // store success or fail

            username = username.Trim();

            email = email.Trim();

            // if no errorsm returns null

            string? usernameError = ValidateUsername(username); 

            string? emailError = ValidateEmail(email);

            string? passwordError = ValidatePassword(password, username, email);

            if (usernameError != null)

            {
                result.IsError = true;

                result.ErrorMessage = usernameError;
            }

            else if (emailError != null)

            {
                result.IsError = true;

                result.ErrorMessage = emailError;
            }

            else if (passwordError != null)

            {
                result.IsError = true;

                result.ErrorMessage = passwordError;
            }

            else if (await _usersStorage.UsernameExistsAsync(username)) // checks if the user's name already exists in
                                                                        // the users storage
            {
                result.IsError = true;

                result.ErrorMessage = "Username is already taken.";
            }
            else if (await _usersStorage.EmailExistsAsync(email))
            {
                result.IsError = true;

                result.ErrorMessage = "Email is already registered.";
            }

            else

            {
                List<User> allUsers = (await _usersStorage.GetAllUsersAsync()).Users;

                // create new id

                // if there are users, take the maximum id and add 1; if there are no users, new id is 1.

                int newId = allUsers.Count > 0 ? allUsers.Max(u => u.Id) + 1 : 1;

                // new user creation

                var user = new User

                {
                    Id = newId,
                    Username = username,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    Role = UserRole.User,
                    IsBlocked = false
                };

                await _usersStorage.AddUserAsync(user);

                var profile = new Profile
                {
                    UserId = user.Id,
                    FullName = string.Empty,
                    Bio = string.Empty,
                    LearningGoal = string.Empty,
                    ShortReviewDays = Constants.DefaultShortReviewDays,
                    NormalReviewDays = Constants.DefaultNormalReviewDays,
                    LongReviewDays = Constants.DefaultLongReviewDays,
                    AnswerTimerSeconds = Constants.DefaultAnswerTimerSeconds
                };

                await _profilesStorage.AddProfileAsync(profile);

                result.SuccessMessage = $"Registration successful! Welcome, {username}.";
            }

            return result;
        }

        public async Task<ActionResult> LoginAsync(string usernameOrEmail, string password)
        {
            var result = new ActionResult();

            usernameOrEmail = usernameOrEmail.Trim();

            if (string.IsNullOrWhiteSpace(usernameOrEmail))

            {
                result.IsError = true;

                result.ErrorMessage = "Username or email is required.";
            }

            else if (string.IsNullOrWhiteSpace(password))

            {
                result.IsError = true;

                result.ErrorMessage = "Password is required.";
            }

            else

            {
                User? user = await _usersStorage.GetByUsernameOrEmailAsync(usernameOrEmail);

                if (user == null)

                {
                    await AppendLoginEventAsync(usernameOrEmail, LoginEventType.Failed, "User not found.");

                    result.IsError = true;

                    result.ErrorMessage = "User not found.";
                }

                else if (user.IsBlocked)

                {
                    await AppendLoginEventAsync(usernameOrEmail, LoginEventType.Blocked, "Login attempt by blocked user.");

                    result.IsError = true;

                    result.ErrorMessage = "Your account is blocked. Contact support.";
                }

                else if (!VerifyPassword(password, user.PasswordHash))

                {
                    await AppendLoginEventAsync(usernameOrEmail, LoginEventType.Failed, "Incorrect password.");

                    result.IsError = true;

                    result.ErrorMessage = "Incorrect password.";
                }

                else

                {
                    await AppendLoginEventAsync(usernameOrEmail, LoginEventType.Success, "Login successful.");

                    CurrentSession.SetUser(user); // stores the user in the current session

                    result.SuccessMessage = $"Welcome back, {user.Username}!";
                }
            }

            return result;
        }

        public ActionResult Logout()
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn) // if no one is logged-in

            {
                result.IsError = true;

                result.ErrorMessage = "No user is currently logged in.";
            }

            else

            {
                CurrentSession.Clear();

                result.SuccessMessage = "You have been logged out.";
            }

            return result;
        }

        public async Task<ActionResult> DeleteAccountAsync(string password)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn) // if no log-in

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else if (!VerifyPassword(password, CurrentSession.LoggedInUser!.PasswordHash)) 
            
            {
                result.IsError = true;

                result.ErrorMessage = "Incorrect password. Account not deleted.";
            }

            else

            {
                int userId = CurrentSession.LoggedInUser!.Id; // get current log-in user id and store in userId

                await _usersStorage.DeleteUserAsync(userId);

                await _profilesStorage.DeleteProfileByUserIdAsync(userId);

                CurrentSession.Clear();

                result.SuccessMessage = "Your account has been deleted successfully.";
            }

            return result;
        }

        public async Task<ProfileResult> GetProfileAsync()
        {
            var result = new ProfileResult();

            if (!CurrentSession.IsLoggedIn) // no log-in

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in to view your profile.";
            }

            else

            {
                Profile? profile = await _profilesStorage.GetProfileByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                if (profile == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Profile not found.";
                }

                else

                {
                    result.Profile = profile;
                }
            }

            return result;
        }

        public async Task<ActionResult> UpdateProfileAsync(string fullName, string bio, string learningGoal, 
            int shortDays, int normalDays, int longDays, int timerSeconds)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in to update your profile.";
            }

            else if (shortDays <= 0 || normalDays <= 0 || longDays <= 0 || timerSeconds <= 0)

            {
                result.IsError = true;

                result.ErrorMessage = "All values must be positive numbers.";
            }

            else

            {
                Profile? profile = await _profilesStorage.GetProfileByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                if (profile == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Profile not found.";
                }

                else

                {
                    profile.FullName = fullName.Trim(); // removing extra spaces

                    profile.Bio = bio.Trim();

                    profile.LearningGoal = learningGoal.Trim();

                    profile.ShortReviewDays = shortDays;

                    profile.NormalReviewDays = normalDays;

                    profile.LongReviewDays = longDays;

                    profile.AnswerTimerSeconds = timerSeconds;

                    var updateResult = await _profilesStorage.UpdateProfileAsync(profile);

                    if (updateResult.IsError)

                    {
                        result.IsError = true;

                        result.ErrorMessage = updateResult.ErrorMessage;
                    }

                    else
                    {
                        result.SuccessMessage = "Profile updated successfully.";
                    }
                }
            }

            return result;
        }


        // 2. Creates a LoginEvent object and saves it to login event storage.
        private async Task AppendLoginEventAsync(string usernameOrEmail, LoginEventType eventType, string message)
        {
            var loginEvent = new LoginEvent

            {
                CreatedAt = DateTime.Now,
                UsernameOrEmail = usernameOrEmail,
                EventType = eventType,
                Message = message
            };

            await _loginEventsStorage.AppendLoginEventAsync(loginEvent);
        }

        public static string? ValidateUsername(string username)
        {
            string? error = null;

            if (string.IsNullOrWhiteSpace(username))

            {
                error = "Username is required.";
            }

            else if (username.Length < Constants.UsernameMinLength || username.Length > Constants.UsernameMaxLength)
            
            {
                error = $"Username must be between {Constants.UsernameMinLength} and {Constants.UsernameMaxLength} characters.";
            }
            
            else if (!char.IsLetter(username[0]))
            
            {
                error = "Username must start with a letter.";
            }
            
            else if (!Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_.]*$"))
            
            {
                error = "Username can only contain letters, digits, underscores, and dots.";
            }

            return error;
        }

        public static string? ValidateEmail(string email)
        {
            string? error = null;

            if (string.IsNullOrWhiteSpace(email))
            
            {
                error = "Email is required.";
            }
            
            else if (email.Contains(' '))
            
            {
                error = "Email must not contain spaces.";
            }
            
            else if (email.Length < Constants.EmailMinLength || email.Length > Constants.EmailMaxLength)
            
            {
                error = $"Email must be between {Constants.EmailMinLength} and {Constants.EmailMaxLength} characters.";
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            
            {
                error = "Email format is invalid.";
            }

            return error;
        }

        public static string? ValidatePassword(string password, string username, string email)
        {
            string? error = null;

            if (string.IsNullOrWhiteSpace(password))
           
            {
                error = "Password is required.";
            }
            
            else if (password.Length < Constants.PasswordMinLength || password.Length > Constants.PasswordMaxLength)
            
            {
                error = $"Password must be between {Constants.PasswordMinLength} and {Constants.PasswordMaxLength} characters.";
            }
            
            else if (password.Contains(' '))
            
            {
                error = "Password must not contain spaces.";
            }
            
            else if (!password.Any(char.IsUpper))
            
            {
                error = "Password must contain at least one uppercase letter.";
            }
            
            else if (!password.Any(char.IsLower))
            
            {
                error = "Password must contain at least one lowercase letter.";
            }
            
            else if (!password.Any(char.IsDigit))
            
            {
                error = "Password must contain at least one digit.";
            }
            
            else if (!password.Any(c => !char.IsLetterOrDigit(c)))
            
            {
                error = "Password must contain at least one special character.";
            }
            
            else if (password.ToLower() == username.ToLower())
            
            {
                error = "Password must not be equal to the username.";
            }
            
            else if (password.ToLower() == email.ToLower())
            
            {
                error = "Password must not be equal to the email.";
            }
            
            else if (GetPasswordStrength(password) == PasswordStrength.Weak)
            
            {
                error = "Password is too weak. Choose a stronger password.";
            }

            return error;
        }

        public static PasswordStrength GetPasswordStrength(string password)
        {
            int score = 0;

            if (password.Length >= 12) { score++; }
            if (password.Length >= 16) { score++; }
            if (password.Any(char.IsUpper)) { score++; }
            if (password.Any(char.IsLower)) { score++; }
            if (password.Any(char.IsDigit)) { score++; }
            if (password.Any(c => !char.IsLetterOrDigit(c))) { score++; }

            PasswordStrength strength = PasswordStrength.Strong;

            if (score <= 3)

            {
                strength = PasswordStrength.Weak;
            }

            else if (score <= 5)

            {
                strength = PasswordStrength.Good;
            }

            return strength;
        }

        public static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            return Convert.ToHexString(bytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}