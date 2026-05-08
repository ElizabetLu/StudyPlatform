using StudyPlatform.Data.FileStorages.Base;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.Flashcards
{
    public class FlashcardsFileStorage
    {
        // 1. Create a private storage object that reads and writes Flashcard objects to the flashcards file.

        private readonly JsonFileStorage<Flashcard> _storage = new JsonFileStorage<Flashcard>(Constants.FilePaths.Flashcards);

        // 2. To make searching by id faster.

        private readonly Dictionary<int, Flashcard> _index = new Dictionary<int, Flashcard>();

        private bool _indexLoaded = false;


        // 3. Checks if the dictionary _index is loaded before using it.

        private async Task EnsureIndexAsync()
        {
            if (!_indexLoaded)

            {
               // gets all flashcards from JSON storage. and clears the old index.
   
                List<Flashcard> flashcards = await _storage.ReadAllAsync();

                _index.Clear();

                foreach (Flashcard flashcard in flashcards)

                {
                    _index[flashcard.Id] = flashcard;
                }

                _indexLoaded = true;
            }
        }

        
        private void InvalidateIndex()
        {
            _indexLoaded = false;
        }

        // 4. Reads all flashcards and returns them in a form of a list.

        public async Task<List<Flashcard>> GetAllFlashcardsAsync()
        {
            List<Flashcard> flashcards = await _storage.ReadAllAsync();

            return flashcards;
        }


        public async Task<List<Flashcard>> GetByUserIdAsync(int userId)
        {
            List<Flashcard> flashcards = await _storage.ReadAllAsync();

            return flashcards.Where(f => f.UserId == userId).ToList();
        }


        // 5. Returns one flashcard by its id. Searches in dictionary.

        public async Task<Flashcard?> GetByIdAsync(int id)
        {
            await EnsureIndexAsync();

            Flashcard? flashcard = _index.ContainsKey(id) ? _index[id] : null;

            return flashcard;
        }



        public async Task AddFlashcardAsync(Flashcard flashcard)
        {
            List<Flashcard> flashcards = await _storage.ReadAllAsync();

            flashcards.Add(flashcard);

            await _storage.WriteAllAsync(flashcards);

            InvalidateIndex();
        }



        public async Task<FileStorageResultBase> UpdateFlashcardAsync(Flashcard updatedFlashcard)
        {
            List<Flashcard> flashcards = await _storage.ReadAllAsync();

            int index = flashcards.FindIndex(f => f.Id == updatedFlashcard.Id); 

            // Create result object

            FileStorageResultBase result = new FileStorageResultBase();

            if (index == -1)

            {
                result.IsError = true;

                result.ErrorMessage = "Flashcard not found.";
            }

            else

            {
                flashcards[index] = updatedFlashcard; // replace the old flashcard in the list

                await _storage.WriteAllAsync(flashcards); // saving updated list

                InvalidateIndex(); // no longer relevant
            }

            return result;
        }

        public async Task DeleteFlashcardAsync(int id)
        {
            List<Flashcard> flashcards = await _storage.ReadAllAsync();

            flashcards.RemoveAll(f => f.Id == id);

            await _storage.WriteAllAsync(flashcards); // saving updated list

            InvalidateIndex();
        }
    }
}