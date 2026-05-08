using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Flashcards;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Users
{
    public class NotesPanel
    {
        // 1. Store refereces to the file storage for using later by a class.

        private readonly NotesFileStorage _notesStorage;

        private readonly FlashcardsFileStorage _flashcardsStorage;


        // Adds notes list.

        public class NotesResult : BusinessLogicResultBase
        {
            public List<Note> Notes { get; set; } = new List<Note>();
        }

        // for create, delete, update actions.

        public class ActionResult : BusinessLogicResultBase
        {
            public string SuccessMessage { get; set; } = string.Empty;
        }

        public NotesPanel(NotesFileStorage notesStorage, FlashcardsFileStorage flashcardsStorage)
        {
            _notesStorage = notesStorage;
            _flashcardsStorage = flashcardsStorage;
        }

        // 2. Gets all notes for the logged-in user.

        public async Task<NotesResult> GetMyNotesAsync()
        {
            var result = new NotesResult(); // creates a result object

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;
                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // load notes for the user

                List<Note> notes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                result.Notes = MergeSort(notes);  // sorts by MergeSort and returns the result
            }

            return result;
        }

        public async Task<ActionResult> CreateNoteAsync(string title, string content, string front, string back, string topic, List<string> tags)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                List<Note> allNotes = await _notesStorage.GetAllNotesAsync(); // get all notes

                int newId = allNotes.Count > 0 ? allNotes.Max(n => n.Id) + 1 : 1; // if there are notes already,
                                                                                  // take the largest id and add 1.
                                                                                  // if no notes exist,
                                                                                  // start from 1

                var note = new Note
                {
                    Id = newId,
                    UserId = CurrentSession.LoggedInUser!.Id,
                    Title = title.Trim(),
                    Content = content.Trim(),
                    Front = front.Trim(),
                    Back = back.Trim(),
                    Topic = topic.Trim(),
                    Tags = tags,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _notesStorage.AddNoteAsync(note);

                result.SuccessMessage = "Note created successfully.";
            }

            return result;
        }

      

        public async Task<ActionResult> UploadFromTxtAsync(string filePath, string topic)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";

            }

            else if (!File.Exists(filePath))

            {
                result.IsError = true;

                result.ErrorMessage = $"File not found: {filePath}";
            }

            else

            {
                string[] lines = await File.ReadAllLinesAsync(filePath); // read all lines from the file

                List<Note> allNotes = await _notesStorage.GetAllNotesAsync(); // load all existing notes

                int nextId = allNotes.Count > 0 ? allNotes.Max(n => n.Id) + 1 : 1; // if there are already note,
                                                                                   // take the biggest existing id
                                                                                   // add 1. If there are no notes, 
                                                                                   // start with id = 1

                int count = 0; // amount of successfully imported notes

                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line)) // ignore empty lines

                    {
                        string[] parts = line.Split('|');

                        string front = parts.Length > 0 ? parts[0].Trim() : line.Trim(); // if there is at least one part,
                                                                                         // use the first part as a front -
                                                                                         // otherwise - the whole line.

                        string back = parts.Length > 1 ? parts[1].Trim() : string.Empty; // if there is a second part after | = use back,
                                                                                         // otherwise back becomes empty string.

                        var note = new Note
                        {
                            Id = nextId++,
                            UserId = CurrentSession.LoggedInUser!.Id,
                            Title = front,
                            Content = line.Trim(),
                            Front = front,
                            Back = back,
                            Topic = topic.Trim(),
                            Tags = new List<string>(),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        await _notesStorage.AddNoteAsync(note);

                        count++;
                    }
                }

                result.SuccessMessage = $"{count} note(s) imported from file successfully.";
            }

            return result;
        }

        public async Task<ActionResult> CreateFlashcardFromNoteAsync(int noteId)
        {
            var result = new ActionResult(); // created an object

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";

            }

            else

            {
                // load all user's notes

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);


                Note? note = userNotes.FirstOrDefault(n => n.Id == noteId);

                if (note == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Note not found.";
                }

                // check for front and back

                else if (string.IsNullOrWhiteSpace(note.Front) || string.IsNullOrWhiteSpace(note.Back))

                {
                    result.IsError = true;

                    result.ErrorMessage = "Note must have both Front and Back fields to create a flashcard.";
                }

                else

                {
                    // load all flashcards

                    List<Flashcard> allFlashcards = await _flashcardsStorage.GetAllFlashcardsAsync();

                    int newId = allFlashcards.Count > 0 ? allFlashcards.Max(f => f.Id) + 1 : 1; // if there are flashcards already,
                                                                                                // take the largest id and add 1.
                                                                                                // if no flashcards exist,
                                                                                                // start from 1

                    var flashcard = new Flashcard
                    {
                        Id = newId,
                        UserId = CurrentSession.LoggedInUser!.Id,
                        Front = note.Front,
                        Back = note.Back,
                        Topic = note.Topic,
                        NextReviewDate = DateTime.Now,
                        ReviewCount = 0
                    };

                    await _flashcardsStorage.AddFlashcardAsync(flashcard);

                    result.SuccessMessage = $"Flashcard created from note '{note.Title}' successfully.";
                }
            }

            return result;
        }

        public async Task<ActionResult> UpdateNoteAsync(int noteId, string title, string content, string front, string back, string topic, List<string> tags)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";

            }

            else

            {
                // load user's current notes

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                Note? note = userNotes.FirstOrDefault(n => n.Id == noteId); // return the first note
                                                                            // with matching id.

                if (note == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Note not found.";
                }

                else
                {
                    if (!string.IsNullOrWhiteSpace(title)) 

                    { note.Title = title.Trim(); }

                    if (!string.IsNullOrWhiteSpace(content)) 

                    { note.Content = content.Trim(); }

                    if (!string.IsNullOrWhiteSpace(front)) 

                    { note.Front = front.Trim(); }

                    if (!string.IsNullOrWhiteSpace(back))
                    
                    { note.Back = back.Trim(); }

                    if (!string.IsNullOrWhiteSpace(topic)) 
                    
                    { note.Topic = topic.Trim(); }

                    if (tags.Count > 0) 
                    
                    { note.Tags = tags; }

                    note.UpdatedAt = DateTime.Now;

                    var updateResult = await _notesStorage.UpdateNoteAsync(note);

                    if (updateResult.IsError)
                    {
                        result.IsError = true;
                        result.ErrorMessage = updateResult.ErrorMessage;
                    }
                    else
                    {
                        result.SuccessMessage = "Note updated successfully.";
                    }
                }
            }

            return result;
        }

        public async Task<ActionResult> DeleteNoteAsync(int noteId)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Load all current user's notes

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                Note? note = userNotes.FirstOrDefault(n => n.Id == noteId);

                if (note == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Note not found.";
                }

                else

                {
                    await _notesStorage.DeleteNoteAsync(noteId);

                    result.SuccessMessage = "Note deleted successfully.";
                }
            }

            return result;
        }

        public async Task<ActionResult> DeleteAllNotesAsync()
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Load all current user's notes

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                foreach (Note note in userNotes)

                {
                    await _notesStorage.DeleteNoteAsync(note.Id);
                }

                result.SuccessMessage = $"{userNotes.Count} note(s) deleted successfully."; // return the amount
                                                                                            // of deleted ones
            }

            return result;
        }

        public async Task<NotesResult> SearchNotesAsync(string searchText)
        {
            var result = new NotesResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else if (string.IsNullOrWhiteSpace(searchText))

            {
                result.IsError = true;

                result.ErrorMessage = "Search text is required.";
            }

            else

            {
                string search = searchText.ToLower().Trim();

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                List<Note> sorted = MergeSort(userNotes);

                BinarySearchByTitle(sorted, search);

                result.Notes = sorted // where the search text is found.
                    .Where(n =>
                        n.Title.ToLower().Contains(search) ||
                        n.Content.ToLower().Contains(search) ||
                        n.Topic.ToLower().Contains(search) ||
                        n.Tags.Any(t => t.ToLower().Contains(search)))
                    .ToList();
            }

            return result;
        }

              

        // if list has 0 or 1 item, already sorted otherwise split list into two halves;
        // sort left half;
        // sort right half
        // merge them together in order

        private List<Note> MergeSort(List<Note> notes)
        {
            List<Note> result;

            if (notes.Count <= 1)

            {
                result = notes;
            }

            else

            {
                int mid = notes.Count / 2;

                List<Note> left = MergeSort(notes.Take(mid).ToList());

                List<Note> right = MergeSort(notes.Skip(mid).ToList());

                result = Merge(left, right);
            }

            return result;
        }

        // compare first items from left and right, take the smaller NextReviewDate, move forward,
        // when one list ends, add remaining items from the other list

        private List<Note> Merge(List<Note> left, List<Note> right)
        {
            var merged = new List<Note>();

            int i = 0;

            int j = 0;

            while (i < left.Count && j < right.Count)

            {
                if (string.Compare(left[i].Title, right[j].Title, StringComparison.OrdinalIgnoreCase) <= 0)

                {
                    merged.Add(left[i]);

                    i++;
                }

                else

                {
                    merged.Add(right[j]);

                    j++;
                }
            }

            while (i < left.Count)

            {
                merged.Add(left[i]);

                i++;
            }

            while (j < right.Count)

            {
                merged.Add(right[j]);

                j++;
            }

            return merged;
        }

        private int BinarySearchByTitle(List<Note> sortedNotes, string searchText)
        {
            int lo = 0;

            int hi = sortedNotes.Count - 1;

            int result = -1;

            while (lo <= hi)

            {
                int mid = (lo + hi) / 2;

                int cmp = string.Compare(sortedNotes[mid].Title.ToLower(), 
                    searchText, StringComparison.OrdinalIgnoreCase);

                if (cmp == 0)

                {
                    result = mid;

                    hi = mid - 1;
                }

                else if (cmp < 0)

                {
                    lo = mid + 1;
                }

                else

                {
                    hi = mid - 1;
                }
            }

            return result;
        }
    }
}