using StudyPlatform.Dictionaries;

namespace StudyPlatform.Models
{
    public class LoginEvent
    {
        public DateTime CreatedAt { get; set; }
        public required string UsernameOrEmail { get; set; }
        public LoginEventType EventType { get; set; }
        public required string Message { get; set; }
    }
}