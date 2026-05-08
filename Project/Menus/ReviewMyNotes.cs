using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class ReviewMyNotesMenu
    {
        // 1. Store the helpers

        private readonly StudyTasksPanel _studyTasksPanel;

        private readonly NotesFileStorage _notesStorage;

        public ReviewMyNotesMenu(StudyTasksPanel studyTasksPanel, NotesFileStorage notesStorage)
        {
            _studyTasksPanel = studyTasksPanel;

            _notesStorage = notesStorage;
        }

        public async Task ShowAsync()
        {
            Console.Clear();

            Console.Write("Enter topic to review: ");

            string topic = Console.ReadLine() ?? string.Empty;

            // Calls GetMyStudyTasksAsync() to get all the user's study tasks

            var tasksResult = await _studyTasksPanel.GetMyStudyTasksAsync();

            if (tasksResult.IsError)

            {
                Console.WriteLine($"Error: {tasksResult.ErrorMessage}");

                Console.ReadLine();
            }

            else

            {
                // filters tasks to only keep ones where the topic matches, 
                // the status is ToBeCompleted — done tasks are excluded

                List<StudyTask> topicTasks = tasksResult.StudyTasks
                    .Where(t => t.Topic.ToLower() == topic.ToLower().
                    Trim() && t.Status == NoteReviewStatus.ToBeCompleted)
                    .ToList();

                // check if any tasks found

                if (topicTasks.Count == 0)
                
                {
                    Console.WriteLine("No notes to be completed found for that topic.");
                   
                    Console.ReadLine();
                }

                else
                
                {
                    Console.WriteLine("Order: 1. By creation order  2. Random order");
                    
                    Console.Write("Choose: ");
                    
                    string orderChoice = Console.ReadLine() ?? string.Empty;

                    List<Note> notes = new List<Note>();

                    foreach (StudyTask task in topicTasks)
                    
                    {
                        // calls GetByIdAsync(task.NoteId) to load the actual note from notes.json.
                        // If the note exists it is added to the notes list. 

                        Note? note = await _notesStorage.GetByIdAsync(task.NoteId);

                        if (note != null)
                        
                        {
                            notes.Add(note);
                        }
                    }

                    if (orderChoice == "2")
                    
                    {
                        var rng = new Random();
                        
                        notes = notes.OrderBy(_ => rng.Next()).ToList();
                    }

                    foreach (Note note in notes)
                    
                    {
                        Console.Clear();
                        
                        Console.WriteLine($"Topic: {note.Topic}");
                        
                        Console.WriteLine($"Title: {note.Title}");
                        
                        Console.WriteLine($"Content: {note.Content}");

                        if (!string.IsNullOrWhiteSpace(note.Front))
                       
                        {
                            Console.WriteLine($"Front: {note.Front}");
                        }

                        if (!string.IsNullOrWhiteSpace(note.Back))
                        
                        {
                            Console.WriteLine($"Back: {note.Back}");
                        }

                        Console.WriteLine();
                       
                        Console.WriteLine("1. Complete");
                       
                        Console.WriteLine("2. Continue later");
                        
                        Console.WriteLine("3. Next");
                        
                        Console.Write("Choose: ");
                        
                        string choice = Console.ReadLine() ?? string.Empty;

                        if (choice == "1")
                       
                        {
                            // calls MarkTaskByNoteIdAsync with NoteReviewStatus.Done
                            // which updates the study task status to Done in studytasks.json

                            await _studyTasksPanel.MarkTaskByNoteIdAsync(note.Id, NoteReviewStatus.Done);
                        }
                        
                        else
                        
                        {
                            //  calls MarkTaskByNoteIdAsync with NoteReviewStatus.ToBeCompleted
                            //  which keeps the task as pending

                            await _studyTasksPanel.MarkTaskByNoteIdAsync(note.Id, NoteReviewStatus.ToBeCompleted);
                        }
                    }

                    Console.WriteLine("Review session complete.");
                    
                    Console.ReadLine();
                }
            }
        }
    }
}