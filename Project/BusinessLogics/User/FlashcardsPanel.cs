using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Base;
using StudyPlatform.Data.FileStorages.Flashcards;
using StudyPlatform.Data.FileStorages.Profiles;
using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Users
{
    public class FlashcardsPanel
    {
        // 1. Store refereces to the file storage for using later by a class.

        private readonly FlashcardsFileStorage _flashcardsStorage;

        private readonly ProfilesFileStorage _profilesStorage;

        // Adds flashcards list.
        
        public class FlashcardsResult : BusinessLogicResultBase
        {
            public List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        }


        // for create, delete, update actions.

        public class ActionResult : BusinessLogicResultBase
        {
            public string SuccessMessage { get; set; } = string.Empty;
        }

        public FlashcardsPanel(FlashcardsFileStorage flashcardsStorage, ProfilesFileStorage profilesStorage)
        {
            _flashcardsStorage = flashcardsStorage;
            _profilesStorage = profilesStorage;
        }

        // 2. Gets all flashcards for the logged-in user.

        public async Task<FlashcardsResult> GetMyFlashcardsAsync()
        {
            var result = new FlashcardsResult(); // creates a result object

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;
                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // load flashcards for the user

                List<Flashcard> flashcards = await _flashcardsStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
                result.Flashcards = MergeSort(flashcards); // sorts by MergeSort and returns the result
            }

            return result;
        }

        public async Task<ActionResult> CreateFlashcardAsync(string front, string back, string topic)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                List<Flashcard> allFlashcards = await _flashcardsStorage.GetAllFlashcardsAsync(); // get all flashcards

                int newId = allFlashcards.Count > 0 ? allFlashcards.Max(f => f.Id) + 1 : 1; // if there are flashcards already,
                                                                                            // take the largest id and add 1.
                                                                                            // if no flashcards exist,
                                                                                            // start from 1

                var flashcard = new Flashcard
                {
                    Id = newId,
                    UserId = CurrentSession.LoggedInUser!.Id,
                    Front = front.Trim(),
                    Back = back.Trim(),
                    Topic = topic.Trim(),
                    NextReviewDate = DateTime.Now,
                    ReviewCount = 0
                };

                await _flashcardsStorage.AddFlashcardAsync(flashcard);

                result.SuccessMessage = "Flashcard created successfully.";
            }

            return result;
        }

        public async Task<ActionResult> UpdateFlashcardAsync(int flashcardId, string front, string back, string topic)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // load user's current flashcards

                List<Flashcard> userCards = await _flashcardsStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                Flashcard? flashcard = userCards.FirstOrDefault(f => f.Id == flashcardId); // return the first card
                                                                                           // with matching id.

                if (flashcard == null)

                {
                    result.IsError = true;
                    result.ErrorMessage = "Flashcard not found.";
                }

                else

                {
                    if (!string.IsNullOrWhiteSpace(front)) 
                    
                    { flashcard.Front = front.Trim(); }

                    if (!string.IsNullOrWhiteSpace(back)) 
                    
                    { flashcard.Back = back.Trim(); }

                    if (!string.IsNullOrWhiteSpace(topic)) 
                    
                    { flashcard.Topic = topic.Trim(); }

                    var updateResult = await _flashcardsStorage.UpdateFlashcardAsync(flashcard);

                    if (updateResult.IsError)

                    {
                        result.IsError = true;
                        result.ErrorMessage = updateResult.ErrorMessage;
                    }

                    else

                    {
                        result.SuccessMessage = "Flashcard updated successfully.";
                    }
                }
            }

            return result;
        }

        public async Task<ActionResult> DeleteFlashcardAsync(int flashcardId)
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                // Load all current user's cards

                List<Flashcard> userCards = await _flashcardsStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);
                
                Flashcard? flashcard = userCards.FirstOrDefault(f => f.Id == flashcardId);

                if (flashcard == null)

                {
                    result.IsError = true;

                    result.ErrorMessage = "Flashcard not found.";
                }

                else

                {
                    await _flashcardsStorage.DeleteFlashcardAsync(flashcardId);

                    result.SuccessMessage = "Flashcard deleted successfully.";
                }
            }

            return result;
        }

        public async Task<ActionResult> DeleteAllFlashcardsAsync()
        {
            var result = new ActionResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else
            {
                // Load all current user's cards

                List<Flashcard> userCards = await _flashcardsStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                foreach (Flashcard flashcard in userCards)
                {
                    await _flashcardsStorage.DeleteFlashcardAsync(flashcard.Id);
                }

                result.SuccessMessage = $"{userCards.Count} flashcard(s) deleted successfully."; // return the amount
                                                                                                 // of deleted ones
            }

            return result;
        }

        // 3. Due - review date is now or already passed.
        public async Task<FlashcardsResult> GetDueFlashcardsAsync()
        {
            var result = new FlashcardsResult();

            if (!CurrentSession.IsLoggedIn)

            {
                result.IsError = true;

                result.ErrorMessage = "You must be logged in.";
            }

            else

            {
                List<Flashcard> userCards = await _flashcardsStorage.GetByUserIdAsync(CurrentSession.LoggedInUser!.Id);

                List<Flashcard> sorted = MergeSort(userCards); // sort by nextreviewdate

                int startIndex = BinarySearchFirstDue(sorted); // to find first due card

                if (startIndex >= 0) // there is at least one due flashcard
                {
                    // ignore the first startIndex items, start reading the list from that position
                    // keep only the flashcards whose review date is now or earlier

                    result.Flashcards = sorted.Skip(startIndex).Where(f => f.NextReviewDate <= DateTime.Now).ToList();
                }
                else
                {
                    // start index was not found, so check the whole list taking all due flashcards

                    result.Flashcards = sorted.Where(f => f.NextReviewDate <= DateTime.Now).ToList();
                }
            }

            return result;
        }

        public async Task<Profile?> GetUserProfileAsync()
        {
            Profile? profile = await _profilesStorage.GetProfileByUserIdAsync(CurrentSession.LoggedInUser!.Id);

            return profile;
        }

        public string ReviewFlashcardWithTimer(Flashcard flashcard, int timerSeconds)
        {
            Console.WriteLine($"Topic: {flashcard.Topic}");

            Console.WriteLine($"Front: {flashcard.Front}");

            Console.WriteLine($"You have {timerSeconds} seconds to type your answer.");

            Console.Write("Your answer: ");

            string answer = ReadLineWithTimeout(timerSeconds); // calls the function

            return answer;
        }

        // 4. Updates flashcard review data after a study session

        public async Task ApplyReviewResultAsync(Flashcard flashcard, FlashcardReviewResult reviewResult, Profile profile)
        {
            flashcard.ReviewCount++;

            flashcard.LastReviewedAt = DateTime.Now;

            flashcard.LastReviewResult = reviewResult;

            if (reviewResult == FlashcardReviewResult.NotPassedShort)
            
            {
                flashcard.NextReviewDate = DateTime.Now.AddDays(profile.ShortReviewDays);
            }

            else if (reviewResult == FlashcardReviewResult.NotPassedNormal)
            
            {
                flashcard.NextReviewDate = DateTime.Now.AddDays(profile.NormalReviewDays);
            }

            else if (reviewResult == FlashcardReviewResult.PassedNormal)
            
            {
                flashcard.NextReviewDate = DateTime.Now.AddDays(profile.NormalReviewDays);
            }

            else
            
            {
                flashcard.NextReviewDate = DateTime.Now.AddDays(profile.LongReviewDays);
            }

            await _flashcardsStorage.UpdateFlashcardAsync(flashcard);
        }

        private string ReadLineWithTimeout(int seconds)
        {
            string result = string.Empty;

            bool answered = false;


            // in order to avoid the console.Readline() for waiting forever until the user presses Enter,
            // we use Thread to run separately from the main program.

            var thread = new System.Threading.Thread(() =>
            {
                // wait for user input, save it into result.

                result = Console.ReadLine() ?? string.Empty;
                answered = true;
            });


            // if the main program ends, this thread should not keep the whole application running

            thread.IsBackground = true;

            // a new thread waits for the input

            thread.Start();

            // we are waiting the thread to finish with the given time. If the user answers in time,
            // thread is finished

            thread.Join(TimeSpan.FromSeconds(seconds));

            if (!answered)

            {
                Console.WriteLine();

                Console.WriteLine("Time is up!");
            }

            return result;
        }

        // if list has 0 or 1 item, already sorted otherwise split list into two halves;
        // sort left half;
        // sort right half
        // merge them together in order

        private List<Flashcard> MergeSort(List<Flashcard> flashcards)
        {
            List<Flashcard> result;

            if (flashcards.Count <= 1)

            {
                result = flashcards;
            }

            else

            {
                int mid = flashcards.Count / 2;

                List<Flashcard> left = MergeSort(flashcards.Take(mid).ToList());

                List<Flashcard> right = MergeSort(flashcards.Skip(mid).ToList());

                result = Merge(left, right);
            }

            return result;
        }

        // compare first items from left and right, take the smaller NextReviewDate, move forward,
        // when one list ends, add remaining items from the other list

        private List<Flashcard> Merge(List<Flashcard> left, List<Flashcard> right)
        {
            var merged = new List<Flashcard>();

            int i = 0;

            int j = 0;

            while (i < left.Count && j < right.Count)
            
            {
                if (left[i].NextReviewDate <= right[j].NextReviewDate)

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

        private int BinarySearchFirstDue(List<Flashcard> sortedByDate)
        {
            int lo = 0;

            int hi = sortedByDate.Count - 1;

            int result = -1;

            while (lo <= hi)

            {
                int mid = (lo + hi) / 2;

                if (sortedByDate[mid].NextReviewDate <= DateTime.Now)

                {
                    result = mid;
                    hi = mid - 1;
                }

                else

                {
                    lo = mid + 1;
                }
            }

            return result;
        }
    }
}