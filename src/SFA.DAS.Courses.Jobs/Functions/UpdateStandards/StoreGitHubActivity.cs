using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public class StoreGitHubActivity
    {
        private readonly ILogger<StoreGitHubActivity> _logger;
        private readonly IGitHubRepositoryService _gitHubRepositoryService;
        private readonly IApprenticeshipStandardsService _apprenticeshipStandardsService;

        public StoreGitHubActivity(
            ILogger<StoreGitHubActivity> logger,
            IGitHubRepositoryService gitHubRepositoryService, 
            IApprenticeshipStandardsService apprenticeshipStandardsService)
        {
            _logger = logger;
            _gitHubRepositoryService = gitHubRepositoryService;
            _apprenticeshipStandardsService = apprenticeshipStandardsService;
        }

        [Function("StoreGitHubActivity")]
        public async Task RunActivity([ActivityTrigger] string name)
        {
            _logger.LogInformation("{ActivityName} started at {DateTimeNow}", nameof(StoreGitHubActivity), DateTime.Now);

            var standards = await _apprenticeshipStandardsService.GetAllStandards();
            var keys = standards.Keys.ToList();

            _logger.LogInformation("Found {KeysCount} standards", keys.Count);

            foreach (var standard in standards)
            {
                try
                {
                    var existingFile = await _gitHubRepositoryService.GetFileInformation(standard.Key, _logger);

                    await _gitHubRepositoryService.UpdateDocument(
                        standard.Key,
                        existingFile,
                        standard.Value,
                        $"{keys.IndexOf(standard.Key) + 1}/{keys.Count}",
                        _logger);
                }
                catch(Exception ex) 
                {
                    _logger.LogInformation(ex, "Unable to process {StandardName} skipping", standard.Key);
                }
            }
        }
    }
}
