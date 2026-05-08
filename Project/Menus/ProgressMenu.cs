using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;
using System.Collections;
using static System.Collections.Specialized.BitVector32;

namespace StudyPlatform.Menus
{
    public class ProgressMenu
    {
        // 1. Store helper

        private readonly ProgressPanel _progressPanel;

        public ProgressMenu(ProgressPanel progressPanel)
        {
            _progressPanel = progressPanel;
        }

        public async Task ShowAsync()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();

                Console.WriteLine("Progress");

                Console.WriteLine("1. My Progress Report");

                Console.WriteLine("2. Back");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)
                {
                    case "1":
                        
                        await HandleProgressReportAsync();
                        
                        break;
                    
                    case "2":
                        
                        running = false;
                       
                        break;
                   
                    default:
                        
                        Console.WriteLine("Invalid option.");
                        
                        Console.ReadLine();
                        
                        break;
                }
            }
        }


        // 2. Progress Report

        private async Task HandleProgressReportAsync()
        {
            Console.Clear();
            
            Console.WriteLine("My Progress Report");
            
            Console.WriteLine("------------------");

            // Calls `GetProgressReportAsync()` from `ProgressPanel` which loads notes,
            // flashcards and study tasks and calculates all the stats.
  
            var result = await _progressPanel.GetProgressReportAsync();

            if (result.IsError)
            
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
           
            else

            // prints the full report in sections

            {
                Console.WriteLine($"Total notes                 : {result.TotalNotes}");
                
                Console.WriteLine($"Total notes to study        : {result.TotalNotesToStudy}");
                
                Console.WriteLine($"Notes to be completed       : {result.NotesToBeCompleted}");
                
                Console.WriteLine($"Notes done                  : {result.NotesDone}");
                
                Console.WriteLine($"Total flashcards            : {result.TotalFlashcards}");
               
                Console.WriteLine($"Due flashcards              : {result.DueFlashcards}");

                if (result.TopHardestFlashcards.Count > 0)
                
                {
                    Console.WriteLine("\nTop Hardest Flashcards:");

                    foreach (Flashcard fc in result.TopHardestFlashcards)
                    
                    {
                        string lastResult = fc.LastReviewResult.HasValue
                            ? fc.LastReviewResult.Value.ToString()
                            : "Not reviewed";
                        
                        Console.WriteLine($"  [{fc.Id}] {fc.Front} | Last result: {lastResult} | Reviews: {fc.ReviewCount}");
                    }
                }

                if (result.TopNotesToBeCompleted.Count > 0)
                
                {
                    Console.WriteLine("\nTop Notes To Be Completed (by deadline):");

                    foreach (StudyTask task in result.TopNotesToBeCompleted)
                    
                    {
                        Console.WriteLine($"  [{task.Id}] {task.Title} ({task.Topic}) - Due: {task.Deadline:yyyy-MM-dd}");
                    }
                }

                if (result.TopOverdueNotes.Count > 0)
                
                {
                    Console.WriteLine("\nTop Overdue Notes:");

                    foreach (StudyTask task in result.TopOverdueNotes)
                    
                    {
                        Console.WriteLine($"  [{task.Id}] {task.Title} ({task.Topic}) - Was due: {task.Deadline:yyyy-MM-dd}");
                    }
                }
            }

            Console.ReadLine();
        }
    }
}