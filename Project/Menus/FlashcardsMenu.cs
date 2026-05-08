using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class FlashcardsMenu
    {
        // 1. Store the helper

        private readonly FlashcardsPanel _flashcardsPanel;

        public FlashcardsMenu(FlashcardsPanel flashcardsPanel)
        {
            _flashcardsPanel = flashcardsPanel;
        }

        public async Task ShowAsync()
        {
            bool running = true;

            while (running)

            {
                Console.Clear();

                Console.WriteLine("My Flashcards");

                Console.WriteLine("1. View My Flashcards");

                Console.WriteLine("2. Create Flashcard");

                Console.WriteLine("3. Update Flashcard");

                Console.WriteLine("4. Delete Flashcard");

                Console.WriteLine("5. Delete All Flashcards");

                Console.WriteLine("6. Review My Flashcards");

                Console.WriteLine("7. Back");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)

                {
                    case "1":

                        await HandleViewFlashcardsAsync();

                        break;

                    case "2":

                        await HandleCreateFlashcardAsync();

                        break;

                    case "3":

                        await HandleUpdateFlashcardAsync();

                        break;

                    case "4":

                        await HandleDeleteFlashcardAsync();

                        break;

                    case "5":

                        await HandleDeleteAllFlashcardsAsync();

                        break;

                    case "6":

                        await HandleReviewFlashcardsAsync();

                        break;

                    case "7":

                        running = false;

                        break;

                    default:

                        Console.WriteLine("Invalid option.");

                        Console.ReadLine();

                        break;
                }
            }
        }

        // 2. Calls GetMyFlashcardsAsync() which loads all flashcards of the user sorted by next review date. 

        private async Task HandleViewFlashcardsAsync()
        {
            Console.Clear();

            var result = await _flashcardsPanel.GetMyFlashcardsAsync();

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else if (result.Flashcards.Count == 0)

            {
                Console.WriteLine("You have no flashcards.");
            }

            else

            {
                foreach (Flashcard fc in result.Flashcards)

                {
                    Console.WriteLine($"[{fc.Id}] {fc.Front} / {fc.Back} ({fc.Topic}) - Next: {fc.NextReviewDate:dd-MM-yyyy}");
                }
            }

            Console.ReadLine();
        }


        // 3. Create flashcards
        private async Task HandleCreateFlashcardAsync()
        {
            Console.Clear();

            Console.WriteLine("Create Flashcard");

            Console.Write("Front: ");

            string front = Console.ReadLine() ?? string.Empty;

            Console.Write("Back: ");

            string back = Console.ReadLine() ?? string.Empty;

            Console.Write("Topic: ");

            string topic = Console.ReadLine() ?? string.Empty;

            // calls CreateFlashcardAsync which saves the new flashcard to flashcards.json with
            // NextReviewDate set to now so it is immediately available for review.

            var result = await _flashcardsPanel.CreateFlashcardAsync(front, back, topic);

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

        private async Task HandleUpdateFlashcardAsync()
        {
            Console.Clear();

            // loads and displays all the user's flashcards. 

            var listResult = await _flashcardsPanel.GetMyFlashcardsAsync(); 

            if (listResult.IsError || listResult.Flashcards.Count == 0)

            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no flashcards.");
                
                Console.ReadLine();
            }

            else
            
            {
                foreach (Flashcard fc in listResult.Flashcards)
                
                {
                    Console.WriteLine($"[{fc.Id}] {fc.Front} / {fc.Back} ({fc.Topic})");
                }

                // Then asks for id to update.

                Console.Write("Enter flashcard ID to update: ");
                
                bool isValidId = int.TryParse(Console.ReadLine(), out int id); // converts a string to the integer,
                                                                              // if success - stores the int in Id

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }
               
                else
                
                {
                    Console.Write("Front (leave blank to keep): ");
                    
                    string front = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Back (leave blank to keep): ");
                    
                    string back = Console.ReadLine() ?? string.Empty;
                   
                    Console.Write("Topic (leave blank to keep): ");
                    
                    string topic = Console.ReadLine() ?? string.Empty;

                    var result = await _flashcardsPanel.UpdateFlashcardAsync(id, front, back, topic);

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

        private async Task HandleDeleteFlashcardAsync()
        {
            Console.Clear();

            //  loads and displays flashcards,

            var listResult = await _flashcardsPanel.GetMyFlashcardsAsync();

            if (listResult.IsError || listResult.Flashcards.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no flashcards.");
                Console.ReadLine();
            }
           
            else
            {
                
                foreach (Flashcard fc in listResult.Flashcards)
                
                {
                    Console.WriteLine($"[{fc.Id}] {fc.Front} / {fc.Back} ({fc.Topic})");
                }

                Console.Write("Enter flashcard ID to delete: ");

                // asks for id to delete
                
                bool isValidId = int.TryParse(Console.ReadLine(), out int id); // converts a string to the integer,
                                                                              // if success - stores the int in Id

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }

                else
                
                {
                    // calls DeleteFlashcardAsync which removes that flashcard from the JSON file.

                    var result = await _flashcardsPanel.DeleteFlashcardAsync(id);

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

        private async Task HandleDeleteAllFlashcardsAsync()
        {
            Console.Clear();
            
            Console.Write("Are you sure you want to delete all your flashcards? (yes/no): ");
           
            string confirm = Console.ReadLine() ?? string.Empty;

            if (confirm.ToLower() != "yes")
            
            {
                Console.WriteLine("Cancelled.");
            }
            
            else
            
            {
                var result = await _flashcardsPanel.DeleteAllFlashcardsAsync();

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

        private async Task HandleReviewFlashcardsAsync()
        {
            Console.Clear();

            //  loads only due flashcards — cards where NextReviewDate is today or earlier.
            //  If none are due it says so.

            var dueResult = await _flashcardsPanel.GetDueFlashcardsAsync();

            if (dueResult.IsError)
            
            {
                Console.WriteLine($"Error: {dueResult.ErrorMessage}");
                
                Console.ReadLine();
            }

            else if (dueResult.Flashcards.Count == 0)
            
            {
                Console.WriteLine("No flashcards due for review.");
                Console.ReadLine();
            }

            else
            
            {
                // It loads the user's profile to get their personal
                // timer and review period settings

                Profile? profile = await _flashcardsPanel.GetUserProfileAsync();
                
                int timerSeconds = profile?.AnswerTimerSeconds ?? 30;
                
                int normalDays = profile?.NormalReviewDays ?? 3;
               
                int longDays = profile?.LongReviewDays ?? 7;
                
                int shortDays = profile?.ShortReviewDays ?? 1;

                // Shows the topic and front of the card, starts a timer

                foreach (Flashcard fc in dueResult.Flashcards)
                
                {
                    Console.Clear();
                    
                    string userAnswer = _flashcardsPanel.ReviewFlashcardWithTimer(fc, timerSeconds);

                    bool timedOut = string.IsNullOrWhiteSpace(userAnswer);
                   
                    bool correct = !timedOut && userAnswer.Trim().ToLower() == fc.Back.Trim().ToLower();

                    Console.WriteLine();

                    if (timedOut)
                    
                    {
                        Console.WriteLine("Result: Not Passed");
                        
                        Console.WriteLine($"Back: {fc.Back}");
                       
                        Console.WriteLine($"Next review in {shortDays} day(s) (Short period).");
                        
                        await _flashcardsPanel.ApplyReviewResultAsync(fc, FlashcardReviewResult.NotPassedShort, profile!);
                    }
                    
                    else if (correct)
                    
                    {
                        Console.WriteLine("Result: Passed");
                        
                        Console.WriteLine($"Back: {fc.Back}");
                        
                        Console.WriteLine($"1. Review in {normalDays} day(s) (Normal)");
                        
                        Console.WriteLine($"2. Review in {longDays} day(s) (Long)");
                        
                        Console.Write("Choose: ");
                        
                        string pick = Console.ReadLine() ?? string.Empty;

                        FlashcardReviewResult reviewResult = pick == "2"
                            ? FlashcardReviewResult.PassedLong
                            : FlashcardReviewResult.PassedNormal;

                        await _flashcardsPanel.ApplyReviewResultAsync(fc, reviewResult, profile!);
                    }

                    else
                    
                    {
                        Console.WriteLine("Result: Not Passed");
                        
                        Console.WriteLine($"Back: {fc.Back}");
                        
                        Console.WriteLine($"1. Review in {shortDays} day(s) (Short)");
                       
                        Console.WriteLine($"2. Review in {normalDays} day(s) (Normal)");
                       
                        Console.Write("Choose: ");
                        
                        string pick = Console.ReadLine() ?? string.Empty;

                        FlashcardReviewResult reviewResult = pick == "2"
                            ? FlashcardReviewResult.NotPassedNormal
                            : FlashcardReviewResult.NotPassedShort;

                        await _flashcardsPanel.ApplyReviewResultAsync(fc, reviewResult, profile!);
                    }

                    Console.ReadLine();
                }

                Console.WriteLine("Review session complete.");
                
                Console.ReadLine();
            }
        }
    }
}