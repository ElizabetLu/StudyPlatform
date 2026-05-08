using StudyPlatform.BusinessLogics.Admin;
using StudyPlatform.BusinessLogics.Auth;
using StudyPlatform.BusinessLogics.Users;
using StudyPlatform.Data.FileStorages.Flashcards;
using StudyPlatform.Data.FileStorages.LoginEvents;
using StudyPlatform.Data.FileStorages.Notes;
using StudyPlatform.Data.FileStorages.Profiles;
using StudyPlatform.Data.FileStorages.StudyTasks;
using StudyPlatform.Data.FileStorages.Users;
using StudyPlatform.Dictionaries;

namespace StudyPlatform.Menus
{
    public class MainMenu
    {
        // 1. Store helpers

        private readonly AuthPanel _authPanel;

        private readonly AdminPanel _adminPanel;

        private readonly UserPanel _userPanel;

        private readonly NotesPanel _notesPanel;

        private readonly FlashcardsPanel _flashcardsPanel;

        private readonly StudyTasksPanel _studyTasksPanel;

        private readonly ProgressPanel _progressPanel;

        private readonly NotesFileStorage _notesStorage;

        public MainMenu()
        {
            // 2. Create storages

            var usersStorage = new UsersFileStorage();

            var profilesStorage = new ProfilesFileStorage();

            var loginEventsStorage = new LoginEventsFileStorage();

            _notesStorage = new NotesFileStorage(); // stored as a field since it needs to be passed
                                                    // to both NotesPanel and ReviewMyNotesMenu later.

            var flashcardsStorage = new FlashcardsFileStorage();

            var studyTasksStorage = new StudyTasksFileStorage();

            // 3. Create Panels that use storages

            _authPanel = new AuthPanel(usersStorage, profilesStorage, loginEventsStorage);

            _adminPanel = new AdminPanel(usersStorage, profilesStorage);

            _userPanel = new UserPanel(_notesStorage, flashcardsStorage);

            _notesPanel = new NotesPanel(_notesStorage, flashcardsStorage);

            _flashcardsPanel = new FlashcardsPanel(flashcardsStorage, profilesStorage);

            _studyTasksPanel = new StudyTasksPanel(studyTasksStorage, _notesStorage);

            _progressPanel = new ProgressPanel(_notesStorage, flashcardsStorage, studyTasksStorage);
        }

        public async Task StartAsync()
        {
            while (true)

            // the only way to exit - Environment.Exit(0) called from AuthMenu when the user chooses Exit

            {
                Console.Clear();

                if (!CurrentSession.IsLoggedIn)

                {
                    // Shows Register, Log In, Exit.
                    // A new AuthMenu object is created on every iteration.

                    await new AuthMenu(_authPanel).ShowAsync();
                }

                else if (CurrentSession.LoggedInUser!.Role == UserRole.Admin)
                
                {
                    // shows only admin's features

                    await new AdminMenu(_adminPanel, _authPanel).ShowAsync();
                }
                
                else
                
                {
                    // shows only user's features

                    await new UserMenu(_authPanel, _userPanel, _notesPanel, _flashcardsPanel,
                        _studyTasksPanel, _progressPanel, _notesStorage).ShowAsync();
                }
            }
        }
    }
}