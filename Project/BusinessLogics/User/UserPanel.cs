using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Flashcards;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Users
{
    public class UserPanel
    {
        // 1. Store refereces to the file storage for using later by a class.

        private readonly NotesFileStorage _notesStorage;

        private readonly FlashcardsFileStorage _flashcardsStorage;

        // Adds items list.

        public class SearchResult : BusinessLogicResultBase
        {
            public List<LearningItem> Items { get; set; } = new List<LearningItem>();
        }


        public UserPanel(NotesFileStorage notesStorage, FlashcardsFileStorage flashcardsStorage)
        {
            _notesStorage = notesStorage;
            _flashcardsStorage = flashcardsStorage;
        }



        public async Task<SearchResult> SearchLibraryAsync(string searchText)
        {
            var result = new SearchResult(); // creates a result object

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

                int userId = CurrentSession.LoggedInUser!.Id; // Gets the id of the logged-in user.

                List<Note> notes = await _notesStorage.GetByUserIdAsync(userId); // load user's notes

                List<Flashcard> flashcards = await _flashcardsStorage.GetByUserIdAsync(userId); // load user's flashcards

                List<LearningItem> foundNotes = notes // keep only those that satisfy the conditions,
                                                      // checks if the field contains the search text.
                                                      // converts note into learning item
                    .Where(n =>
                        n.Title.ToLower().Contains(search) ||
                        n.Content.ToLower().Contains(search) ||
                        n.Topic.ToLower().Contains(search) ||
                        n.Tags.Any(t => t.ToLower().Contains(search)))
                    .Cast<LearningItem>()
                    .ToList();

                List<LearningItem> foundFlashcards = flashcards
                    .Where(f =>
                        f.Front.ToLower().Contains(search) ||
                        f.Back.ToLower().Contains(search) ||
                        f.Topic.ToLower().Contains(search))
                    .Cast<LearningItem>()
                    .ToList();

                result.Items = foundNotes.Concat(foundFlashcards).ToList(); // combine 
            }

            return result;
        }
    }
}