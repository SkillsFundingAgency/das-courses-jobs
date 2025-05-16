using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public static class UpdateStandardsOrchestration
    {
        [Function(nameof(UpdateStandardsOrchestration))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger(typeof(UpdateStandardsOrchestration));
            logger.LogInformation("{OrchestrationName} started", nameof(UpdateStandardsOrchestration));

            var allStandards = await context.CallActivityAsync<Dictionary<string, string>>(nameof(GetAllStandardsActivity), new TaskOptions());
            var standardsToProcess = new Dictionary<string, string>(allStandards);
            var retryLimit = 3;
            var attempt = 0;

            while (standardsToProcess.Any() && attempt++ < retryLimit)
            {
                logger.LogInformation("Attempt {Attempt} - processing {Count} standards", attempt, standardsToProcess.Count);

                var failedKeys = await context.CallActivityAsync<List<string>>(nameof(StoreGitHubBatchActivity), standardsToProcess);

                standardsToProcess = failedKeys.ToDictionary(key => key, key => allStandards[key]);

                logger.LogInformation("Attempt {Attempt} complete - {Failed} failures", attempt, failedKeys.Count);
            }

            if (standardsToProcess.Any())
            {
                logger.LogWarning("Processing completed with {Count} failures after {Retries} retries", standardsToProcess.Count, retryLimit);
            }
            else
            {
                logger.LogInformation("All standards processed successfully");
            }

            logger.LogInformation("{OrchestrationName} completed", nameof(UpdateStandardsOrchestration));
        }
    }
}