using StudyPlatform.Dictionaries;
using StudyPlatform.Models;

namespace StudyPlatform.Data.FileStorages.LoginEvents
{
    public class LoginEventsFileStorage
    {
        private readonly string _filePath = Constants.FilePaths.LoginEvents;
        

        // 1. Adding login event to the log file.

        public async Task AppendLoginEventAsync(LoginEvent loginEvent)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            string line = $"[{loginEvent.CreatedAt:dd-MM-yyyy HH:mm:ss}] " +
                          $"[{loginEvent.EventType}] " +
                          $"User: {loginEvent.UsernameOrEmail} | " +
                          $"{loginEvent.Message}";

            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine); // adds a line break,
                                                                                  // so each event goes onto a new line.
        }
    }
}