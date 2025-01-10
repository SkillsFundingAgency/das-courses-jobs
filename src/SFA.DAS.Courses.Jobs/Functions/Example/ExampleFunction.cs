using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Functions.Example
{
    public class ExampleFunction
    {
        private readonly ILogger<ExampleFunction> _logger;
        
        public ExampleFunction(ILogger<ExampleFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ExampleTimer))]
        public async Task ExampleTimer([TimerTrigger("%ExampleTimerSchedule%")] TimerInfo myTimer,
            [DurableClient] DurableTaskClient client)
        {
            await Run(nameof(ExampleTimer), client);
        }

#if DEBUG
        [Function(nameof(ExampleHttp))]
        public async Task ExampleHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request,
            [DurableClient] DurableTaskClient client)
        {
            await Run(nameof(ExampleHttp), client);
        }
#endif

        private async Task Run(string functionName, [DurableClient] DurableTaskClient client)
        {
            try
            {
                _logger.LogInformation("{FunctionName} has been triggered", functionName);

                string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ExampleOrchestration), CancellationToken.None);

                _logger.LogInformation("{FunctionName} has started orchestration with {InstanceId}", functionName, instanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{FunctionName} has has failed", functionName);
            }
        }
    }
}
