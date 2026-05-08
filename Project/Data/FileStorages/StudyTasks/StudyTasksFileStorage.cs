using StudyPlatform.Data.FileStorages.Base;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.StudyTasks
{
    public class StudyTasksFileStorage
    {
        // 1. Create a private storage object that reads and writes StudyTasks objects to the StudyTasks file.

        private readonly JsonFileStorage<StudyTask> _storage = new JsonFileStorage<StudyTask>(Constants.FilePaths.StudyTasks);

        // 2. To make searching by id faster.

        private readonly Dictionary<int, StudyTask> _index = new Dictionary<int, StudyTask>();
        
        private bool _indexLoaded = false;

        // 3. Checks if the dictionary _index is loaded before using it.

        private async Task EnsureIndexAsync()
        {
            if (!_indexLoaded)

            {
                // gets all studytasks from JSON storage. and clears the old index.

                List<StudyTask> tasks = await _storage.ReadAllAsync();

                _index.Clear();

                foreach (StudyTask task in tasks)

                {
                    _index[task.Id] = task;
                }

                _indexLoaded = true;
            }
        }

        private void InvalidateIndex()
        {
            _indexLoaded = false;
        }

        // 4. Reads all studytasks and returns them in a form of a list.

        public async Task<List<StudyTask>> GetAllStudyTasksAsync()
        {
            List<StudyTask> tasks = await _storage.ReadAllAsync();

            return tasks;
        }

        public async Task<List<StudyTask>> GetByUserIdAsync(int userId)
        {
            List<StudyTask> tasks = await _storage.ReadAllAsync();
            return tasks.Where(t => t.UserId == userId).ToList();
        }

        // 5. Returns one studytask by its id. Searches in dictionary.

        public async Task<StudyTask?> GetByIdAsync(int id)
        {
            await EnsureIndexAsync();

            StudyTask? task = _index.ContainsKey(id) ? _index[id] : null;

            return task;
        }

        public async Task AddStudyTaskAsync(StudyTask studyTask)
        {
            List<StudyTask> tasks = await _storage.ReadAllAsync();

            tasks.Add(studyTask);

            await _storage.WriteAllAsync(tasks);

            InvalidateIndex();
        }

        public async Task<FileStorageResultBase> UpdateStudyTaskAsync(StudyTask updatedTask)
        {
            List<StudyTask> tasks = await _storage.ReadAllAsync();

            int index = tasks.FindIndex(t => t.Id == updatedTask.Id);

            // Create result object

            FileStorageResultBase result = new FileStorageResultBase();

            if (index == -1)

            {
                result.IsError = true;

                result.ErrorMessage = "Study task not found.";
            }

            else

            {
                tasks[index] = updatedTask; // replace the old studytask in the list

                await _storage.WriteAllAsync(tasks); // saving updated list

                InvalidateIndex(); // no longer relevant
            }

            return result;
        }

        public async Task DeleteStudyTaskAsync(int id)
        {
            List<StudyTask> tasks = await _storage.ReadAllAsync();

            tasks.RemoveAll(t => t.Id == id);

            await _storage.WriteAllAsync(tasks); // saving updated list

            InvalidateIndex();
        }
    }
}