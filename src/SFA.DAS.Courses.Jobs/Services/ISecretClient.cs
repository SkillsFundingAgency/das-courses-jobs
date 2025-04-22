namespace SFA.DAS.Courses.Jobs.Services
{
    public interface ISecretClient
    {
        Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken);
    }

}
