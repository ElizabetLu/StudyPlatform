namespace StudyPlatform.Dictionaries
{
    public class Constants
    {
        public const int UsernameMinLength = 3;

        public const int UsernameMaxLength = 20;

        public const int EmailMinLength = 6;

        public const int EmailMaxLength = 100;

        public const int PasswordMinLength = 8;

        public const int PasswordMaxLength = 50;

        public const int DefaultShortReviewDays = 1;

        public const int DefaultNormalReviewDays = 3;

        public const int DefaultLongReviewDays = 7;

        public const int DefaultAnswerTimerSeconds = 30;

        public static readonly string[] WeakPasswordBlacklist =
        {
            "password", "123456", "abc123", "welcome"
        };

        public class FilePaths
        {
            public const string Users = "Data/users.json";

            public const string Profiles = "Data/profiles.json";

            public const string LoginEvents = "Data/logs.txt";

            public const string Notes = "Data/notes.json";

            public const string Flashcards = "Data/flashcards.json";

            public const string StudyTasks = "Data/studytasks.json";

            public const string StudyPlans = "Data/studyplans.json";
        }
    }
}