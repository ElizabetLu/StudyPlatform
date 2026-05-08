using StudyPlatform.BusinessLogics.Admin;
using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class AdminMenu
    {
        // 1, Creating variables that store helpers

        private readonly AdminPanel _adminPanel;

        private readonly AuthPanel _authPanel;

        public AdminMenu(AdminPanel adminPanel, AuthPanel authPanel)
        {
            _adminPanel = adminPanel;
            _authPanel = authPanel;
        }

        public async Task ShowAsync()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();

                Console.WriteLine($"Admin Panel | {CurrentSession.LoggedInUser!.Username}");

                Console.WriteLine("1. View All Users");

                Console.WriteLine("2. Block User");

                Console.WriteLine("3. Unblock User");

                Console.WriteLine("4. Delete User");

                Console.WriteLine("5. Change User Role");

                Console.WriteLine("6. Delete My Account");

                Console.WriteLine("7. Log Out");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty; // if the left side is null, use the right side



                switch (choice)
                {
                    case "1":

                        await HandleViewAllUsersAsync();

                        break;

                    case "2":

                        await HandleBlockUserAsync();

                        break;

                    case "3":

                        await HandleUnblockUserAsync();

                        break;

                    case "4":

                        await HandleDeleteUserAsync();

                        break;

                    case "5":

                        await HandleChangeUserRoleAsync();

                        break;

                    case "6":
                        await HandleDeleteAccountAsync();

                        running = false;

                        break;

                    case "7":

                        HandleLogout();

                        running = false;

                        break;

                    default:

                        Console.WriteLine("Invalid option.");

                        Console.ReadLine();

                        break;
                }
            }
        }

        // 2. Print all users

        private async Task HandleViewAllUsersAsync()
        {
            Console.Clear();

            Console.WriteLine("All Users");

            var result = await _adminPanel.GetAllUsersWithProfilesAsync();

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else

            {
                foreach ((User user, Profile? profile) in result.UsersWithProfiles)

                {
                    string blocked = user.IsBlocked ? "[BLOCKED]" : string.Empty;

                    string fullName = profile?.FullName ?? "(no profile)";

                    Console.WriteLine($"{user.Id} | {user.Username} | {user.Email} | {user.Role} {blocked} | {fullName}");
                }
            }

            Console.ReadLine();
        }

        // 3. Blocking user

        private async Task HandleBlockUserAsync()
        {
            Console.Clear();

            Console.Write("Enter User ID to block: ");

            string input = Console.ReadLine() ?? string.Empty;

            bool isValid = int.TryParse(input, out int userId); // converts a string to the integer,
                                                                // if success - stores the int in userId

            if (!isValid)

            {
                Console.WriteLine("Invalid ID.");
            }

            else

            {
                var result = await _adminPanel.BlockUserAsync(userId); // block user

                if (result.IsError)

                {
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                }

                else

                {
                    Console.WriteLine(result.SuccessMessage); 
                }
            }

            Console.ReadLine();
        }

        // 4. Unblocking user

        private async Task HandleUnblockUserAsync()
        {
            Console.Clear();

            Console.Write("Enter User ID to unblock: ");

            string input = Console.ReadLine() ?? string.Empty; // if the left side is null, use the right side

            bool isValid = int.TryParse(input, out int userId);  // converts a string to the integer,
                                                                 // if success - stores the int in userId


            if (!isValid)

            {
                Console.WriteLine("Invalid ID.");
            }

            else

            {
                var result = await _adminPanel.UnblockUserAsync(userId); // unblock the user

                if (result.IsError)

                {
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                }

                else

                {
                    Console.WriteLine(result.SuccessMessage);
                }
            }

            Console.ReadLine();
        }


        // 5. Deleting a user

        private async Task HandleDeleteUserAsync()
        {
            Console.Clear();

            Console.Write("Enter User ID to delete: ");

            string input = Console.ReadLine() ?? string.Empty;

            bool isValid = int.TryParse(input, out int userId); // converts a string to the integer,
                                                                // if success - stores the int in userId

            if (!isValid)

            {
                Console.WriteLine("Invalid ID.");
            }

            else

            {
                var result = await _adminPanel.DeleteUserAsync(userId); // delete the user

                if (result.IsError)

                {
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                }

                else

                {
                    Console.WriteLine(result.SuccessMessage);
                }
            }

            Console.ReadLine();
        }


        // 6. Changing status

        private async Task HandleChangeUserRoleAsync()
        {
            Console.Clear();

            Console.Write("Enter User ID: ");

            string input = Console.ReadLine() ?? string.Empty;

            bool isValidId = int.TryParse(input, out int userId);  // converts a string to the integer,
                                                                   // if success - stores the int in userId

            if (!isValidId)

            {
                Console.WriteLine("Invalid ID.");
            }

            else

            {
                Console.Write("New Role (0 = User, 1 = Admin): ");

                string roleInput = Console.ReadLine() ?? string.Empty;

                bool isValidRole = int.TryParse(roleInput, out int roleInt); // converts a string to the integer,
                                                                             // if success - stores the int in userId


                if (!isValidRole || !Enum.IsDefined(typeof(UserRole), roleInt)) // is a given number valid to UserRole

                {
                    Console.WriteLine("Invalid role.");
                }

                else

                {
                    UserRole newRole = (UserRole)roleInt; // change the role

                    var result = await _adminPanel.ChangeUserRoleAsync(userId, newRole); 

                    if (result.IsError)

                    {
                        Console.WriteLine($"Error: {result.ErrorMessage}");
                    }

                    else

                    {
                        Console.WriteLine(result.SuccessMessage);
                    }
                }
            }

            Console.ReadLine();
        }

        // 7. Deleting account

        private async Task HandleDeleteAccountAsync()
        {
            Console.Clear();

            Console.WriteLine("Delete My Account");

            Console.WriteLine("This action is permanent and cannot be undone.");

            Console.Write("Enter your password to confirm: ");

            string password = Console.ReadLine() ?? string.Empty;

            var result = await _authPanel.DeleteAccountAsync(password);

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else

            {
                Console.WriteLine(result.SuccessMessage);
            }

            Console.ReadLine();
        }

        // 8. Log out

        private void HandleLogout()
        {
            var result = _authPanel.Logout();

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else

            {
                Console.WriteLine(result.SuccessMessage);
            }

            Console.ReadLine();
        }
    }
}