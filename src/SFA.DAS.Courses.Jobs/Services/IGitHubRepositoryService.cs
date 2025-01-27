using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Services
{
    public interface IGitHubRepositoryService
    {
        Task<(string? Sha, string? Content)> GetFileInformation(string fileNamePrefix, ILogger log);
        Task UpdateDocument(string fileNamePrefix, (string? Sha, string? Content) existingFile, string updatedContent, string logProgress, ILogger log);
    }
}