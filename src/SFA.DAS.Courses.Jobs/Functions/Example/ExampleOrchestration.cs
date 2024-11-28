using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Functions.Example
{
    public static class ExampleOrchestration
    {
        [Function(nameof(ExampleOrchestration))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(typeof(ExampleOrchestration));

            logger.LogInformation("{OrchestrationName} started", nameof(ExampleOrchestration));

            await context.CallActivityAsync(nameof(ExampleActivity), null, null);
        }
    }
}
