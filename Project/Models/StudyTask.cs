using StudyPlatform.Dictionaries;

namespace StudyPlatform.Models
{
    public class StudyTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public NoteReviewStatus Status { get; set; } = NoteReviewStatus.ToBeCompleted;
        public StudyTaskPriority Priority { get; set; }
    }
}