namespace SFA.DAS.Courses.Jobs.Exceptions
{
    public class GitHubFileException : Exception
    {
        public GitHubFileException(string message)
            : base(message)
        {
        }

        public GitHubFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
