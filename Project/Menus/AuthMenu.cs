using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class AuthMenu
    {

        // 1. Store the helper

        private readonly AuthPanel _authPanel;

        public AuthMenu(AuthPanel authPanel)
        {
            _authPanel = authPanel;
        }

        public async Task ShowAsync()
        {
            Console.WriteLine("Study Platform");

            Console.WriteLine("1. Register");

            Console.WriteLine("2. Log In");

            Console.WriteLine("3. Exit");

            Console.Write("Choose: ");

            string choice = Console.ReadLine() ?? string.Empty;

            switch (choice)
            {
                case "1":

                    await HandleRegisterAsync();

                    break;

                case "2":

                    await HandleLoginAsync();

                    break;

                case "3":

                    Environment.Exit(0);

                    break;

                default:

                    Console.WriteLine("Invalid option.");

                    Console.ReadLine();

                    break;
            }
        }

        private async Task HandleRegisterAsync()
        {
            Console.Clear();

            Console.WriteLine("Register");

            Console.Write("Username: ");

            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Email: ");

            string email = Console.ReadLine() ?? string.Empty;

            Console.Write("Password: ");

            string password = Console.ReadLine() ?? string.Empty;

            var result = await _authPanel.RegisterAsync(username, email, password); // validates and saves information

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

        private async Task HandleLoginAsync()
        {
            Console.Clear();

            Console.WriteLine("Log In");

            Console.Write("Username or Email: ");

            string usernameOrEmail = Console.ReadLine() ?? string.Empty;

            Console.Write("Password: ");

            string password = Console.ReadLine() ?? string.Empty;

            // checks if the user exists, if he is blocked or the password is correct.

            var result = await _authPanel.LoginAsync(usernameOrEmail, password);

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