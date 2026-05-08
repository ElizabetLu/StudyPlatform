using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Models;

namespace StudyPlatform.Menus
{
    public class NotesMenu
    {
        // 1. Store the helper

        private readonly NotesPanel _notesPanel;

        public NotesMenu(NotesPanel notesPanel)
        {
            _notesPanel = notesPanel;
        }

        public async Task ShowAsync()
        {
            bool running = true;

            while (running)

            {
                Console.Clear();

                Console.WriteLine("My Notes");

                Console.WriteLine("1. View My Notes");

                Console.WriteLine("2. Create Note");

                Console.WriteLine("3. Upload Notes from text.txt");

                Console.WriteLine("4. Create Flashcard from Note");

                Console.WriteLine("5. Update Note");

                Console.WriteLine("6. Delete Note");

                Console.WriteLine("7. Delete All Notes");

                Console.WriteLine("8. Search Notes");

                Console.WriteLine("9. Back");

                Console.Write("Choose: ");

                string choice = Console.ReadLine() ?? string.Empty;

                switch (choice)
                {
                    case "1":

                        await HandleViewNotesAsync();

                        break;

                    case "2":

                        await HandleCreateNoteAsync();

                        break;

                    case "3":

                        await HandleUploadFromTxtAsync();

                        break;

                    case "4":

                        await HandleCreateFlashcardFromNoteAsync();

                        break;

                    case "5":

                        await HandleUpdateNoteAsync();

                        break;

                    case "6":

                        await HandleDeleteNoteAsync();

                        break;

                    case "7":

                        await HandleDeleteAllNotesAsync();

                        break;

                    case "8":

                        await HandleSearchNotesAsync();

                        break;

                    case "9":

                        running = false;

                        break;

                    default:

                        Console.WriteLine("Invalid option.");

                        Console.ReadLine();

                        break;
                }
            }
        }


        // 2. Load all user notes by alphabet.

        private async Task HandleViewNotesAsync()
        {
            Console.Clear();

            var result = await _notesPanel.GetMyNotesAsync();

            if (result.IsError)

            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }

            else if (result.Notes.Count == 0)
            
            {
                Console.WriteLine("You have no notes.");
            }
            
            else
            
            {
                foreach (Note note in result.Notes)
                
                {
                    Console.WriteLine($"[{note.Id}] {note.Title} ({note.Topic}) | Front: {note.Front} | Back: {note.Back} | {note.UpdatedAt:yyyy-MM-dd}");
                }
            }

            Console.ReadLine();
        }

      
        // 3. Create notes

        private async Task HandleCreateNoteAsync()
        {
            Console.Clear();
            
            Console.WriteLine("Create Note");
            
            Console.Write("Title: ");
            
            string title = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Content: ");
            
            string content = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Front (question side for flashcard): ");
           
            string front = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Back (answer side for flashcard): ");
           
            string back = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Topic: ");
           
            string topic = Console.ReadLine() ?? string.Empty;
           
            Console.Write("Tags (comma-separated): ");
           
            string tagsInput = Console.ReadLine() ?? string.Empty;
            
            List<string> tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

            // Calls CreateNoteAsync which saves the new note to notes.json.

            var result = await _notesPanel.CreateNoteAsync(title, content, front, back, topic, tags); 

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

        // 4. Upload from a document

        private async Task HandleUploadFromTxtAsync()
        {
            Console.Clear();
            
            Console.WriteLine("Upload Notes from text.txt");
           
            Console.WriteLine("Format: each line is one note.");
           
            Console.WriteLine("To set Front and Back use: Front text | Back text");
           
            Console.Write("File path (default: text.txt): ");
            
            string filePath = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            
            {
                filePath = "text.txt";
            }

            // The path is cleaned with Trim('"') to
            // remove any surrounding quotes the user may have typed.

            filePath = filePath.Trim('"').Trim();

            Console.Write("Topic for all imported notes: ");
            
            string topic = Console.ReadLine() ?? string.Empty;

            // Calls UploadFromTxtAsync which reads the file line by line and creates one note per line. 

            var result = await _notesPanel.UploadFromTxtAsync(filePath, topic);

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

        // 5. Create Flashcards from Notes

        private async Task HandleCreateFlashcardFromNoteAsync()
        {
            Console.Clear();

            // Loads and displays all the user's notes showing their ID, title, front and back.

            var listResult = await _notesPanel.GetMyNotesAsync();

            if (listResult.IsError || listResult.Notes.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no notes.");
                Console.ReadLine();
            }
            
            else
            
            {
                foreach (Note note in listResult.Notes)
                
                {
                    Console.WriteLine($"[{note.Id}] {note.Title} | Front: {note.Front} | Back: {note.Back}");
                }

                Console.Write("Enter note ID to create flashcard from: ");
               
                // validate id

                bool isValidId = int.TryParse(Console.ReadLine(), out int id); // converts a string to the integer,
                                                                               // if success - stores the int in Id

                if (!isValidId)
               
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                }
               
                else
                
                {
                    // calls CreateFlashcardFromNoteAsync which copies the Front and Back
                    // from the note into a new flashcard and saves it to flashcards.json.

                    var result = await _notesPanel.CreateFlashcardFromNoteAsync(id);

                    
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


        // 6. Update Note

        private async Task HandleUpdateNoteAsync()
        {
            Console.Clear();

            // Loads and displays all notes.

            var listResult = await _notesPanel.GetMyNotesAsync();

            if (listResult.IsError || listResult.Notes.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no notes.");
                
                Console.ReadLine();
            }
           
            else
            
            {
                foreach (Note note in listResult.Notes)
                
                {
                    Console.WriteLine($"[{note.Id}] {note.Title} ({note.Topic})");
                }

                Console.Write("Enter note ID to update: ");
                
                // validate id

                bool isValidId = int.TryParse(Console.ReadLine(), out int id); // converts a string to the integer,
                                                                               // if success - stores the int in Id

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }
                else
                {
                    
                    Console.Write("Title (leave blank to keep): ");
                   
                    string title = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Content (leave blank to keep): ");
                    
                    string content = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Front (leave blank to keep): ");
                    
                    string front = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Back (leave blank to keep): ");
                    
                    string back = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Topic (leave blank to keep): ");
                    
                    string topic = Console.ReadLine() ?? string.Empty;
                    
                    Console.Write("Tags comma-separated (leave blank to keep): ");
                    
                    string tagsInput = Console.ReadLine() ?? string.Empty;
                   
                    List<string> tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

                   // calls UpdateNoteAsync which saves the changes.

                    var result = await _notesPanel.UpdateNoteAsync(id, title, content, front, back, topic, tags);

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

        // 7. Delete note

        private async Task HandleDeleteNoteAsync()
        {
            Console.Clear();

            // Loads and displays all notes

            var listResult = await _notesPanel.GetMyNotesAsync();

            if (listResult.IsError || listResult.Notes.Count == 0)
            
            {
                Console.WriteLine(listResult.IsError ? $"Error: {listResult.ErrorMessage}" : "You have no notes.");
                Console.ReadLine();
            }
            
            else
            
            {
                foreach (Note note in listResult.Notes)
                
                {
                    Console.WriteLine($"[{note.Id}] {note.Title} ({note.Topic})");
                }

                Console.Write("Enter note ID to delete: ");
                
                // validate id

                bool isValidId = int.TryParse(Console.ReadLine(), out int id);  // converts a string to the integer,
                                                                                // if success - stores the int in Id

                if (!isValidId)
                
                {
                    Console.WriteLine("Invalid ID.");
                    
                    Console.ReadLine();
                }
                
                else
                
                {
                    // calls DeleteNoteAsync which removes that specific note from notes.json.

                    var result = await _notesPanel.DeleteNoteAsync(id);

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

        // 8. Delete All Notes

        private async Task HandleDeleteAllNotesAsync()
        {
            Console.Clear();
           
            Console.Write("Are you sure you want to delete all your notes? (yes/no): ");
            
            string confirm = Console.ReadLine() ?? string.Empty;

            if (confirm.ToLower() != "yes")
            
            {
                Console.WriteLine("Cancelled.");
            }
            
            else
            {
                //  calls DeleteAllNotesAsync which loops through
                //  all the user's notes and deletes them one by one.

                var result = await _notesPanel.DeleteAllNotesAsync();

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

        // 9. Search

        private async Task HandleSearchNotesAsync()
        {
            Console.Clear();
            
            Console.Write("Search: ");
            
            string searchText = Console.ReadLine() ?? string.Empty;

            // Calls SearchNotesAsync which sorts notes,
            // filters notes 

            var result = await _notesPanel.SearchNotesAsync(searchText);

            if (result.IsError)
            
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
           
            else if (result.Notes.Count == 0)
           
            {
                Console.WriteLine("No notes found.");
            }
            
            else
            {
                foreach (Note note in result.Notes)
               
                {
                    // shows all notes

                    Console.WriteLine($"[{note.Id}] {note.Title} ({note.Topic})"); 
                }
            }

            Console.ReadLine();
        }
    }
}