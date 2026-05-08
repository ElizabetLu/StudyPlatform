using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Flashcards;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Data.FileStorages.StudyTasks;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Users
{
    public class ProgressPanel
    {
        // 1. Store refereces to the file storage for using later by a class.

        private readonly NotesFileStorage _notesStorage;

        private readonly FlashcardsFileStorage _flashcardsStorage;

        private readonly StudyTasksFileStorage _studyTasksStorage;

        // report that would be returned

        public class ProgressResult : BusinessLogicResultBase
        {
            public int TotalNotes { get; set; }

            public int TotalNotesToStudy { get; set; }

            public int NotesToBeCompleted { get; set; }

            public int NotesDone { get; set; }

            public int TotalFlashcards { get; set; }

            public int DueFlashcards { get; set; }

            public List<Flashcard> TopHardestFlashcards { get; set; } = new List<Flashcard>();

            public List<StudyTask> TopNotesToBeCompleted { get; set; } = new List<StudyTask>();

            public List<StudyTask> TopOverdueNotes { get; set; } = new List<StudyTask>();
        }

        public ProgressPanel(NotesFileStorage notesStorage, FlashcardsFileStorage flashcardsStorage,
            StudyTasksFileStorage studyTasksStorage)
        {
            _notesStorage = notesStorage;
            _flashcardsStorage = flashcardsStorage;
            _studyTasksStorage = studyTasksStorage;
        }

        // 2. Get a progress summary

        public async Task<ProgressResult> GetProgressReportAsync()
        {
            var result = new ProgressResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                int userId = CurrentSession.LoggedInUser!.Id; // get the id of the logged-in user

                // Load all user data from the storage

                List<Note> notes = await _notesStorage.GetByUserIdAsync(userId);

                List<Flashcard> flashcards = await _flashcardsStorage.GetByUserIdAsync(userId);

                List<StudyTask> tasks = await _studyTasksStorage.GetByUserIdAsync(userId);

                // Total amount of Notes and Notes to study

                result.TotalNotes = notes.Count;

                result.TotalNotesToStudy = tasks.Count;

                // Amounr of Notes to be completed / done

                result.NotesToBeCompleted = tasks.Count(t => t.Status == NoteReviewStatus.ToBeCompleted);

                result.NotesDone = tasks.Count(t => t.Status == NoteReviewStatus.Done);

                // Total amount of Flashcards and Due Flashcards

                result.TotalFlashcards = flashcards.Count;

                result.DueFlashcards = flashcards.Count(f => f.NextReviewDate <= DateTime.Now);

                result.TopHardestFlashcards = flashcards // already have a result, smaller scores - harder flashcards
                    .Where(f => f.LastReviewResult.HasValue)
                    .OrderBy(f => GetFlashcardDifficultyScore(f.LastReviewResult!.Value))
                    .Take(5)
                    .ToList();

                List<StudyTask> pendingTasks = tasks
                    .Where(t => t.Status == NoteReviewStatus.ToBeCompleted)
                    .ToList();

                result.TopNotesToBeCompleted = pendingTasks
                    .OrderBy(t => t.Deadline)
                    .Take(5)
                    .ToList();

                result.TopOverdueNotes = pendingTasks
                    .Where(t => t.Deadline < DateTime.Now)
                    .OrderBy(t => t.Deadline)
                    .Take(5)
                    .ToList();
            }

            return result;
        }
        
        // The hardest cards appear on top - lower score comes first (since orderby)

        private int GetFlashcardDifficultyScore(FlashcardReviewResult reviewResult)
        {
            int score = 4;

            if (reviewResult == FlashcardReviewResult.NotPassedShort)

            {
                score = 1;
            }

            else if (reviewResult == FlashcardReviewResult.NotPassedNormal)
            
            {
                score = 2;
            }

            else if (reviewResult == FlashcardReviewResult.PassedNormal)
            
            {
                score = 3;
            }

            return score;
        }
    }
}