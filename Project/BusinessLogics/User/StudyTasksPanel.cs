using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Data.FileStorages.StudyTasks;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Users
{
    public class StudyTasksPanel
    {

        // 1. Store refereces to the file storage for using later by a class.

        private readonly StudyTasksFileStorage _studyTasksStorage;

        private readonly NotesFileStorage _notesStorage;


        // Adds studytasks list.

        public class StudyTasksResult : BusinessLogicResultBase
        {
            public List<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
        }

        // for create, delete, update actions.

        public class ActionResult : BusinessLogicResultBase
        {
            public string SuccessMessage { get; set; } = string.Empty;
        }

        public StudyTasksPanel(StudyTasksFileStorage studyTasksStorage, NotesFileStorage notesStorage)
        {
            _studyTasksStorage = studyTasksStorage;
            _notesStorage = notesStorage;
        }

        // 2. Gets all studytasks for the logged-in user.

        public async Task<StudyTasksResult> GetMyStudyTasksAsync()
        {
            var result = new StudyTasksResult(); // creates a result object

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // load all tasks for the current user

                List<StudyTask> tasks = await _studyTasksStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
                result.StudyTasks = MergeSort(tasks); // sorts by MergeSort and returns the result
            }

            return result;
        }

        public async Task<ActionResult> CreateStudyTaskAsync(string noteTitle, string topic, DateTime deadline, StudyTaskPriority priority)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)
            
            {
                result.IsError = true;
                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // load all notes of the current logged-in user.

                List<Note> userNotes = await _notesStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id); // get all studytasks

                // Searching for the user's note.

                Note? note = userNotes.FirstOrDefault(n =>
                    n.Title.ToLower() == noteTitle.ToLower().Trim() &&
                    n.Topic.ToLower() == topic.ToLower().Trim());

                if (note == null)
                
                {
                    result.IsError = true;
                    
                    result.ErrorMessage = "No note found with that title and topic.";
                }
                
                else
                
                {
                    // Get all existing tasks from storage

                    List<StudyTask> allTasks = await _studyTasksStorage.GetAllStudyTasksAsync();
                    
                    int newId = allTasks.Count > 0 ? allTasks.Max(t => t.Id) + 1 : 1; // if there are tasks already,
                                                                                      // take the largest id and add 1.
                                                                                      // if no tasks exist,
                                                                                      // start from 1

                    var task = new StudyTask
                    {
                        Id = newId,
                        UserId = CurrentSession.LoggedInUser!.Id,
                        NoteId = note.Id,
                        Title = note.Title,
                        Topic = note.Topic,
                        Deadline = deadline,
                        Priority = priority,
                        Status = NoteReviewStatus.ToBeCompleted
                    };

                    await _studyTasksStorage.AddStudyTaskAsync(task);

                    result.SuccessMessage = "Study task created successfully.";
                }
            }

            return result;
        }

        public async Task<ActionResult> UpdateStudyTaskStatusAsync(int taskId, NoteReviewStatus status)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Gets all study tasks

                List<StudyTask> userTasks = await _studyTasksStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
                StudyTask? task = userTasks.FirstOrDefault(t => t.Id == taskId); // find task by id

                if (task == null)
                
                {
                    result.IsError = true;

                    result.ErrorMessage = "Study task not found.";
                }

                else

                {
                    task.Status = status; // change task status

                    var updateResult = await _studyTasksStorage.UpdateStudyTaskAsync(task); // save the updated task

                    if (updateResult.IsError)
                    
                    {
                        result.IsError = true;

                        result.ErrorMessage = updateResult.ErrorMessage;
                    }

                    else
                    
                    {
                        result.SuccessMessage = "Study task updated successfully.";
                    }
                }
            }

            return result;
        }

        public async Task<ActionResult> DeleteStudyTaskAsync(int taskId)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Get all tasks

                List<StudyTask> userTasks = await _studyTasksStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
               
                StudyTask? task = userTasks.FirstOrDefault(t => t.Id == taskId); // search task by id

                if (task == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Study task not found.";
                }

                else

                {
                    await _studyTasksStorage.DeleteStudyTaskAsync(taskId);

                    result.SuccessMessage = "Study task deleted successfully.";
                }
            }

            return result;
        }

        public async Task<ActionResult> MarkTaskByNoteIdAsync(int noteId, NoteReviewStatus status)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Get all tasks

                List<StudyTask> userTasks = await _studyTasksStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
                // Search task by id and status

                StudyTask? task = userTasks.FirstOrDefault(t => t.NoteId == noteId && t.Status == NoteReviewStatus.ToBeCompleted);

                if (task != null)

                {
                    task.Status = status; // change task status

                    await _studyTasksStorage.UpdateStudyTaskAsync(task); 
                }
            }

            return result;
        }

        // if list has 0 or 1 item, already sorted otherwise split list into two halves;
        // sort left half;
        // sort right half
        // merge them together in order

        private List<StudyTask> MergeSort(List<StudyTask> tasks)
        {
            List<StudyTask> result;

            if (tasks.Count <= 1)

            {
                result = tasks;
            }

            else

            {
                int mid = tasks.Count / 2;

                List<StudyTask> left = MergeSort(tasks.Take(mid).ToList());

                List<StudyTask> right = MergeSort(tasks.Skip(mid).ToList());

                result = Merge(left, right);
            }

            return result;
        }

        // compare first items from left and right, take the smaller NextReviewDate, move forward,
        // when one list ends, add remaining items from the other list

        private List<StudyTask> Merge(List<StudyTask> left, List<StudyTask> right)
        {
            
            var merged = new List<StudyTask>();

            int i = 0;

            int j = 0;

            while (i < left.Count && j < right.Count)

            {
                if ((int)left[i].Priority >= (int)right[j].Priority)

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
    }
}