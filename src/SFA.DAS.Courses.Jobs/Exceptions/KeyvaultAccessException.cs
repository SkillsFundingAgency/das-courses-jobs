namespace SFA.DAS.Courses.Jobs.Exceptions
{
    public class KeyvaultAccessException : Exception
    {
        public KeyvaultAccessException(string message)
            : base(message)
        {
        }

        public KeyvaultAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
