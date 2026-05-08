using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class UserMenu
    {
        // 1. Store the helpers

        private readonly AuthPanel _authPanel;

        private readonly UserPanel _userPanel;

        private readonly NotesPanel _notesPanel;

        private readonly FlashcardsPanel _flashcardsPanel;

        private readonly StudyTasksPanel _studyTasksPanel;

        private readonly ProgressPanel _progressPanel;

        private readonly NotesFileStorage _notesStorage;

        public UserMenu(AuthPanel authPanel, UserPanel userPanel, NotesPanel notesPanel,
            FlashcardsPanel flashcardsPanel, StudyTasksPanel studyTasksPanel,
            ProgressPanel progressPanel, NotesFileStorage notesStorage)
        {
            _authPanel = authPanel;

            _userPanel = userPanel;

            _notesPanel = notesPanel;

            _flashcardsPanel = flashcardsPanel;

            _studyTasksPanel = studyTasksPanel;

            _progressPanel = progressPanel;

            _notesStorage = notesStorage;
        }



        public async Task ShowAsync()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();

                Console.WriteLine($"Hello, {CurrentSession.LoggedInUser!.Username}");

                Console.WriteLine("1.  My Profile");

                Console.WriteLine("2.  Update Profile");

                Console.WriteLine("3.  My Notes");

                Console.WriteLine("4.  My Flashcards");

                Console.WriteLine("5.  Review My Flashcards");

                Console.WriteLine("6.  My Study Tasks");

                Console.WriteLine("7.  Review My Notes");

                Console.WriteLine("8.  Search Library");

                Console.WriteLine("9.  My Progress Report");

                Console.WriteLine("10. Delete My Account");

                Console.WriteLine("11. Log Out");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)
                {
                    case "1":

                        await HandleViewProfileAsync();

                        break;

                    case "2":

                        await HandleUpdateProfileAsync();

                        break;

                    case "3":

                        await new NotesMenu(_notesPanel).ShowAsync();

                        break;

                    case "4":

                        await new FlashcardsMenu(_flashcardsPanel).ShowAsync();

                        break;

                    case "5":

                        await new FlashcardsMenu(_flashcardsPanel).ShowAsync();

                        break;

                    case "6":

                        await new StudyTasksMenu(_studyTasksPanel).ShowAsync();

                        break;

                    case "7":

                        await new ReviewMyNotesMenu(_studyTasksPanel, _notesStorage).ShowAsync();

                        break;

                    case "8":

                        await HandleSearchLibraryAsync();

                        break;

                    case "9":

                        await new ProgressMenu(_progressPanel).ShowAsync();

                        break;

                    case "10":

                        await HandleDeleteAccountAsync();

                        running = false;

                        break;

                    case "11":

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

        // 2. Profile

        private async Task HandleViewProfileAsync()
        {
            Console.Clear();

            Console.WriteLine("Your Profile");

            // Calls GetProfileAsync() from AuthPanel.

            var result = await _authPanel.GetProfileAsync();

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else
            {
                // Prints all profile fields

                Profile profile = result.Profile!;
                
                Console.WriteLine($"Full Name        : {(string.IsNullOrEmpty(profile.FullName) ? "(not set)" : profile.FullName)}");
                
                Console.WriteLine($"Bio              : {(string.IsNullOrEmpty(profile.Bio) ? "(not set)" : profile.Bio)}");
                
                Console.WriteLine($"Goal             : {(string.IsNullOrEmpty(profile.LearningGoal) ? "(not set)" : profile.LearningGoal)}");
                
                Console.WriteLine($"Short review     : {profile.ShortReviewDays} day(s)");
                
                Console.WriteLine($"Normal review    : {profile.NormalReviewDays} day(s)");
                
                Console.WriteLine($"Long review      : {profile.LongReviewDays} day(s)");
               
                Console.WriteLine($"Answer timer     : {profile.AnswerTimerSeconds} second(s)");
            }

            Console.ReadLine();
        }

        // 3. Update Profile

        private async Task HandleUpdateProfileAsync()
        {
            Console.Clear();
            
            Console.WriteLine("Update Profile");
            
            Console.Write("Full Name: ");
            
            string fullName = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Bio: ");
            
            string bio = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Learning Goal: ");
            
            string learningGoal = Console.ReadLine() ?? string.Empty;
            
            Console.Write("Short review period (days): ");

            bool parsedShort = int.TryParse(Console.ReadLine(), out int shortDays); // converts a string to the integer,
                                                                                   // if success - stores the int,

            Console.Write("Normal review period (days): ");
            
            bool parsedNormal = int.TryParse(Console.ReadLine(), out int normalDays);
           
            Console.Write("Long review period (days): ");
           
            bool parsedLong = int.TryParse(Console.ReadLine(), out int longDays);
            
            Console.Write("Answer timer (seconds): ");
            
            bool parsedTimer = int.TryParse(Console.ReadLine(), out int timerSeconds);

            // If the parse failed - a safe default is used 

            if (!parsedShort || shortDays <= 0) 
            
            { 
                shortDays = 1;
            }
            
            if (!parsedNormal || normalDays <= 0)
            
            { 
                normalDays = 3; 
            }
            if (!parsedLong || longDays <= 0) 
            
            { 
                longDays = 7;
            }

            if (!parsedTimer || timerSeconds <= 0) 
            
            { 
                timerSeconds = 30;
            }

            //  calls UpdateProfileAsync which saves all the changes to profiles.json.

            var result = await _authPanel.UpdateProfileAsync(fullName, bio, learningGoal,
                shortDays, normalDays, longDays, timerSeconds);

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

        // 4. Search Library

        private async Task HandleSearchLibraryAsync()
        {
            Console.Clear();

            Console.Write("Search: ");

            string searchText = Console.ReadLine() ?? string.Empty;

            // Calls SearchLibraryAsync from UserPanel which searches through both notes and flashcards

            var result = await _userPanel.SearchLibraryAsync(searchText);

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else
           
            {
                // results are split into two separate lists 
                // both Note and Flashcard are stored as LearningItem
                // then cast back to their real type for display.

                List<LearningItem> notes = result.Items.Where(i => i is Note).ToList();
                
                List<LearningItem> flashcards = result.Items.Where(i => i is Flashcard).ToList();

                Console.WriteLine($"Notes ({notes.Count}):");

                foreach (LearningItem item in notes)
                
                {
                    Note note = (Note)item;
                    
                    Console.WriteLine($"  [{note.Id}] {note.Title} ({note.Topic})");
                }

                Console.WriteLine($"Flashcards ({flashcards.Count}):");

                foreach (LearningItem item in flashcards)
                
                {
                    Flashcard fc = (Flashcard)item;
                    
                    Console.WriteLine($"  [{fc.Id}] {fc.Front} ({fc.Topic})");
                }
            }

            Console.ReadLine();
        }

        // 5. Delete Account

        private async Task HandleDeleteAccountAsync()
        {
            Console.Clear();
            
            Console.WriteLine("Delete My Account");
            
            Console.WriteLine("This action is permanent and cannot be undone.");
            
            Console.Write("Enter your password to confirm: ");
            
            string password = Console.ReadLine() ?? string.Empty;

            //  Calls DeleteAccountAsync from AuthPanel which verifies the password,
            //  deletes the user and their profile from the JSON files, clears the session.

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

        // 6. Log Out

        private void HandleLogout()
        {
            // Calls Logout() from AuthPanel which simply clears CurrentSession. 

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