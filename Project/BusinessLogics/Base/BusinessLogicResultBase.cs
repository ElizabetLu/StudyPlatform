namespace StudyPlatform.BusinessLogics.Base
{
    public class BusinessLogicResultBase
    {
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}