namespace StudyPlatform.Models
{
    public class Profile
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string LearningGoal { get; set; } = string.Empty;
        public int ShortReviewDays { get; set; } = 1;
        public int NormalReviewDays { get; set; } = 3;
        public int LongReviewDays { get; set; } = 7;
        public int AnswerTimerSeconds { get; set; } = 30;
    }
}