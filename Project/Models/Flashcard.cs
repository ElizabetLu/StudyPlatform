using StudyPlatform.Dictionaries;

namespace StudyPlatform.Models
{
    public class Flashcard : LearningItem
    {
        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;
        public DateTime NextReviewDate { get; set; }
        public int ReviewCount { get; set; }
        public DateTime? LastReviewedAt { get; set; } // can be stored "null" as well
        public FlashcardReviewResult? LastReviewResult { get; set; }
    }
}