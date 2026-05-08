namespace StudyPlatform.Dictionaries
{
    public enum UserRole
    {
        User,

        Admin
    }

    public enum LoginEventType
    {
        Success,

        Failed,

        Blocked
    }

    public enum PasswordStrength
    {
        Weak,

        Good,

        Strong
    }

    public enum StudyTaskPriority
    {
        Low,

        Medium,

        High
    }

    public enum FlashcardReviewResult
    {
        NotPassedShort,

        NotPassedNormal,

        PassedNormal,

        PassedLong
    }

    public enum ReviewPeriodType
    {
        Short,

        Normal,

        Long
    }

    public enum NoteReviewStatus
    {
        ToBeCompleted,

        Done
    }
}