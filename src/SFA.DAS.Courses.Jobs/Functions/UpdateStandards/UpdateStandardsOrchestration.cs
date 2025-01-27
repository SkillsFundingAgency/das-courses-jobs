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
            ILogger logger = context.CreateReplaySafeLogger(typeof(UpdateStandardsOrchestration));

            logger.LogInformation("{OrchestrationName} started", nameof(UpdateStandardsOrchestration));

            await context.CallActivityAsync(nameof(StoreGitHubActivity),
                null,
                new TaskOptions(new RetryPolicy(3, TimeSpan.FromMinutes(10))));
        }
    }
}
