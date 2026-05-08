using StudyPlatform.Models;

namespace StudyPlatform.BusinessLogics.Auth
{
    public static class CurrentSession
    {
        public static User? LoggedInUser { get; private set; } // belongs to class itself,
                                                               // can contain user or null object;
        public static bool IsLoggedIn => LoggedInUser != null; // return true if LoggedInUser is not null,
                                                               // otherwise - false
        public static bool WelcomeShown { get; private set; } = false;

        public static void SetUser(User user)
        {
            LoggedInUser = user;
            WelcomeShown = false;
        }

        public static void MarkWelcomeShown()
        {
            WelcomeShown = true;
        }

        public static void Clear() // clears the current session
        {
            LoggedInUser = null;
            WelcomeShown = false;
        }
    }
}