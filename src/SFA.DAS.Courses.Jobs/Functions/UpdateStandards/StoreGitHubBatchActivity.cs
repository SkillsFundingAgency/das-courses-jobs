using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public class StoreGitHubBatchActivity
    {
        private readonly ILogger<StoreGitHubBatchActivity> _logger;
        private readonly IGitHubRepositoryService _gitHubRepositoryService;

        public StoreGitHubBatchActivity(
            ILogger<StoreGitHubBatchActivity> logger,
            IGitHubRepositoryService gitHubRepositoryService)
        {
            _logger = logger;
            _gitHubRepositoryService = gitHubRepositoryService;
        }

        [Function(nameof(StoreGitHubBatchActivity))]
        public async Task<List<string>> Run([ActivityTrigger] Dictionary<string, string> standards)
        {
            var failed = new List<string>();
            int index = 0;
            int total = standards.Count;

            foreach (var (key, value) in standards)
            {
                try
                {
                    var logProgress = $"{++index}/{total}";
                    _logger.LogInformation("{Progress} Processing {Key}", logProgress, key);

                    var existingFile = await _gitHubRepositoryService.GetFileInformation(key);

                    await _gitHubRepositoryService.UpdateDocument(
                        key,
                        existingFile,
                        value,
                        logProgress);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Processing failed {Key}", key);
                    failed.Add(key);
                }
            }

            return failed;
        }
    }
}
