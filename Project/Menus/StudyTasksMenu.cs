using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class StudyTasksMenu
    {
        // 1. Store the helper
        
        private readonly StudyTasksPanel _studyTasksPanel;

        public StudyTasksMenu(StudyTasksPanel studyTasksPanel)
        {
            _studyTasksPanel = studyTasksPanel;
        }

        public async Task ShowAsync()
        {
            bool running = true;

            while (running)

            {
                Console.Clear();

                Console.WriteLine("My Study Tasks");

                Console.WriteLine("1. View My Study Tasks");

                Console.WriteLine("2. Create Study Task (from note)");

                Console.WriteLine("3. Update Study Task Status");

                Console.WriteLine("4. Delete Study Task");

                Console.WriteLine("5. Back");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)
                {
                    case "1":

                        await HandleViewTasksAsync();

                        break;

                    case "2":

                        await HandleCreateTaskAsync();

                        break;

                    case "3":

                        await HandleUpdateTaskStatusAsync();

                        break;

                    case "4":

                        await HandleDeleteTaskAsync();

                        break;

                    case "5":

                        running = false;

                        break;

                    default:

                        Console.WriteLine("Invalid option.");

                        Console.ReadLine();

                        break;
                }
            }
        }
        
        // 2. View Tasks

        private async Task HandleViewTasksAsync()
        {
            Console.Clear();

            // Calls GetMyStudyTasksAsync() which loads all study tasks sorted from High to Low

            var result = await _studyTasksPanel.GetMyStudyTasksAsync();

            if (result.IsError)
            
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
            
            else if (result.StudyTasks.Count == 0)
           
            {
                Console.WriteLine("You have no study tasks.");
            }
            
            else
            
            {
                foreach (StudyTask task in result.StudyTasks)
                
                {
                    string status = task.Status == NoteReviewStatus.Done ? "[Done]" : "[To be completed]";
                    
                    Console.WriteLine($"[{task.Id}] {task.Title} ({task.Topic}) - {task.Priority} - Due: {task.Deadline:yyyy-MM-dd} {status}");
                }
            }

            Console.ReadLine();
        }


        // 3. Create task

        private async Task HandleCreateTaskAsync()
        {
            Console.Clear();

            //  a study task is always linked to an existing note.
            //  Title and Topic are used to find note

            Console.WriteLine("Create Study Task from Note");
           
            Console.WriteLine("The note must already exist in My Notes.");
            
            Console.Write("Note title: ");
           
            string noteTitle = Console.ReadLine() ?? string.Empty;
            
            Console.Write("Note topic: ");
            
            string topic = Console.ReadLine() ?? string.Empty;
            
            Console.Write("Deadline (dd-MM-yyyy): ");
           
            bool parsedDeadline = DateTime.TryParse(Console.ReadLine(), out DateTime deadline);
            
            Console.Write("Priority (0 = Low, 1 = Medium, 2 = High): ");
            
            bool parsedPriority = Enum.TryParse(Console.ReadLine(), out StudyTaskPriority priority);

            if (!parsedDeadline) 
            
            { 
                deadline = DateTime.Now.AddDays(1);
            }
            
            if (!parsedPriority) 
            
            { 
                priority = StudyTaskPriority.Low;
            }

            // if note is found - StudyTask is created linked to that note's ID and saved to studytasks.json.

            var result = await _studyTasksPanel.CreateStudyTaskAsync(noteTitle, topic, deadline, priority);

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

        // 4. Update Task Status

        private async Task HandleUpdateTaskStatusAsync()
        {
            Console.Clear();

            // Loads and displays all study tasks with their current status. 

            var listResult = await _studyTasksPanel.GetMyStudyTasksAsync();

            if (listResult.IsError || listResult.StudyTasks.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no study tasks.");
                
                Console.ReadLine();
            }
            
            else
            
            {
                foreach (StudyTask task in listResult.StudyTasks)
               
                {
                    string status = task.Status == NoteReviewStatus.Done ? "[Done]" : "[To be completed]";
                    
                    Console.WriteLine($"[{task.Id}] {task.Title} ({task.Topic}) {status}");
                }

                Console.Write("Enter task ID to update: ");
                
                bool isValidId = int.TryParse(Console.ReadLine(), out int id);
                
                Console.Write("New status (0 = To be completed, 1 = Done): ");
               
                // validate id

                bool isValidStatus = int.TryParse(Console.ReadLine(), out int statusInt); // converts a string to the integer,
                                                                               // if success - stores the int

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }
                
                else
                
                {
                    // If valid it asks for a new status — 0 for
                    // To be completed or 1 for Done.

                    if (!isValidStatus) 
                   
                    { 
                        statusInt = 0; 
                    }

                    NoteReviewStatus newStatus = statusInt == 1 ? NoteReviewStatus.Done : NoteReviewStatus.ToBeCompleted;

                    //  calls UpdateStudyTaskStatusAsync which saves the updated status to the file.

                    var result = await _studyTasksPanel.UpdateStudyTaskStatusAsync(id, newStatus);

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
        
        // Delete Task 

        private async Task HandleDeleteTaskAsync()
        {
            Console.Clear();

            //Loads and displays all study tasks. 

            var listResult = await _studyTasksPanel.GetMyStudyTasksAsync();

            if (listResult.IsError || listResult.StudyTasks.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no study tasks.");
                
                Console.ReadLine();
            }
            
            else
            {
                foreach (StudyTask task in listResult.StudyTasks)
                
                {
                    Console.WriteLine($"[{task.Id}] {task.Title} ({task.Topic})");
                }

                Console.Write("Enter task ID to delete: ");

                // validate id
                
                bool isValidId = int.TryParse(Console.ReadLine(), out int id); // converts a string to the integer,
                                                                               // if success - stores the int id

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }
                
                else
                
                {
                    //  calls DeleteStudyTaskAsync which removes that task from studytasks.json

                    var result = await _studyTasksPanel.DeleteStudyTaskAsync(id);

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
    }
}