namespace SFA.DAS.Courses.Jobs.Services
{
    public interface IGitHubRepositoryService
    {
        Task<(string? Sha, string? Content)> GetFileInformation(string fileNamePrefix);
        Task UpdateDocument(string fileNamePrefix, (string? Sha, string? Content) existingFile, string updatedContent, string logProgress);
    }
}