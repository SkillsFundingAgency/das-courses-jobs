using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Services;
using SFA.DAS.Courses.Jobs.TaskQueue;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public class UpdateStandardsFunction
    {
        private readonly ILogger<UpdateStandardsFunction> _logger;
        private readonly UpdateStandardsConfiguration _configuration;
        private readonly IApprenticeshipStandardsService _apprenticeshipStandardsService;
        private readonly IGitHubRepositoryService _gitHubRepositoryService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public UpdateStandardsFunction(
            IOptions<ApplicationConfiguration> configuration,
            ILogger<UpdateStandardsFunction> logger,
            IApprenticeshipStandardsService apprenticeshipStandardsService,
            IGitHubRepositoryService gitHubRepositoryService,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _configuration = configuration.Value.FunctionsConfiguration.UpdateStandardsConfiguration;
            _apprenticeshipStandardsService = apprenticeshipStandardsService;
            _gitHubRepositoryService = gitHubRepositoryService;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        [Function(nameof(UpdateStandardsTimer))]
        public void UpdateStandardsTimer([TimerTrigger("%UpdateStandardsTimerSchedule%")] TimerInfo timerInfo)
        {
            if (_configuration.Enabled)
                QueueUpdateStandards();
        }

#if DEBUG
        [Function(nameof(UpdateStandardsHttp))]
        public void UpdateStandardsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest req)
        {
            if (_configuration.Enabled)
                QueueUpdateStandards();
        }
#endif

        private void QueueUpdateStandards()
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(
                    async ct => await RunUpdateStandards(),
                    nameof(RunUpdateStandards),
                    (duration, log) =>
                    {
                        log.LogInformation("Completed request to {FunctionName} in {Duration}", nameof(RunUpdateStandards), duration);
                    });
        }

        public async Task RunUpdateStandards()
        {
            _logger.LogInformation("UpdateStandards function started");

            var allStandards = await _apprenticeshipStandardsService.GetAllStandards();

            _logger.LogInformation("Retrieved {Count} standards", allStandards.Count);

            var toProcess = new Dictionary<string, string>(allStandards);
            const int retryLimit = 3;
            int attempt = 0;

            while (toProcess.Any() && attempt++ < retryLimit)
            {
                _logger.LogInformation("Attempt {Attempt} - processing {Count} standards", attempt, toProcess.Count);
                var failed = new Dictionary<string, string>();
                int index = 0;
                int total = toProcess.Count;

                foreach (var (key, value) in toProcess)
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
                        _logger.LogWarning(ex, "Failed to process standard {Key}", key);
                        failed[key] = value;
                    }
                }

                toProcess = failed;
            }

            if (toProcess.Any())
            {
                _logger.LogWarning("UpdateStandards completed with {Count} failures after {Retries} attempts", toProcess.Count, retryLimit);
            }
            else
            {
                _logger.LogInformation("UpdateStandards completed successfully");
            }
        }
    }
}
