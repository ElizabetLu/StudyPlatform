using StudyPlatform.Data.FileStorages.Base;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.Notes
{
    public class NotesFileStorage
    {
        // 1. Create a private storage object that reads and writes Notes objects to the notes file.

        private readonly JsonFileStorage<Note> _storage = new JsonFileStorage<Note>(Constants.FilePaths.Notes);

        // 2. To make searching by id faster.

        private readonly Dictionary<int, Note> _index = new Dictionary<int, Note>();

        private bool _indexLoaded = false;

        // 3. Checks if the dictionary _index is loaded before using it.

        private async Task EnsureIndexAsync()
        {
            if (!_indexLoaded)

            {
                // gets all notes from JSON storage. and clears the old index.

                List<Note> notes = await _storage.ReadAllAsync();

                _index.Clear();

                foreach (Note note in notes)
                {
                    _index[note.Id] = note;
                }

                _indexLoaded = true;
            }
        }

        private void InvalidateIndex()
        {
            _indexLoaded = false;
        }

        // 4. Reads all notes and returns them in a form of a list.

        public async Task<List<Note>> GetAllNotesAsync()
        {
            List<Note> notes = await _storage.ReadAllAsync();

            return notes;
        }

        public async Task<List<Note>> GetByUserIdAsync(int userId)
        {
            List<Note> notes = await _storage.ReadAllAsync();

            return notes.Where(n => n.UserId == userId).ToList();
        }


        // 5. Returns one note by its id. Searches in dictionary.

        public async Task<Note?> GetByIdAsync(int id)
        {
            await EnsureIndexAsync();

            Note? note = _index.ContainsKey(id) ? _index[id] : null;

            return note;
        }

        public async Task AddNoteAsync(Note note)
        {
            List<Note> notes = await _storage.ReadAllAsync();

            notes.Add(note);

            await _storage.WriteAllAsync(notes);

            InvalidateIndex();
        }

        public async Task<FileStorageResultBase> UpdateNoteAsync(Note updatedNote)
        {
            List<Note> notes = await _storage.ReadAllAsync();

            int index = notes.FindIndex(n => n.Id == updatedNote.Id);

            // Create result object

            FileStorageResultBase result = new FileStorageResultBase();

            if (index == -1)

            {
                result.IsError = true;

                result.ErrorMessage = "Note not found.";
            }

            else

            {
                notes[index] = updatedNote; // replace the old note in the list

                await _storage.WriteAllAsync(notes); // saving updated list

                InvalidateIndex(); // no longer relevant
            }

            return result;
        }

        public async Task DeleteNoteAsync(int id)
        {
            List<Note> notes = await _storage.ReadAllAsync();

            notes.RemoveAll(n => n.Id == id);

            await _storage.WriteAllAsync(notes); // saving updated list

            InvalidateIndex();
        }
    }
}