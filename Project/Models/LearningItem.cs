namespace StudyPlatform.Models
{
    public abstract class LearningItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Topic { get; set; } = string.Empty;
    }
}