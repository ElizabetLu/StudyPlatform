namespace StudyPlatform.Data.FileStorages.Base
{
    public class FileStorageResultBase
    {
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}