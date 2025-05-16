namespace SFA.DAS.Courses.Jobs.Exceptions
{
    public class StoreGitHubActivityException : Exception
    {
        public StoreGitHubActivityException(string message)
            : base(message)
        {
        }

        public StoreGitHubActivityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
