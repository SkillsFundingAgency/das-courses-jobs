using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public class UpdateStandardsFunction
    {
        private readonly ILogger<UpdateStandardsFunction> _logger;
        
        public UpdateStandardsFunction(ILogger<UpdateStandardsFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(UpdateStandardsTimer))]
        public async Task UpdateStandardsTimer([TimerTrigger("%UpdateStandardsTimerSchedule%")] TimerInfo myTimer,
            [DurableClient] DurableTaskClient client)
        {
            await Run(nameof(UpdateStandardsTimer), client);
        }

#if DEBUG
        [Function(nameof(UpdateStandardsHttp))]
        public async Task UpdateStandardsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request,
            [DurableClient] DurableTaskClient client)
        {
            await Run(nameof(UpdateStandardsHttp), client);
        }
#endif

        private async Task Run(string functionName, [DurableClient] DurableTaskClient client)
        {
            try
            {
                _logger.LogInformation("{FunctionName} has been triggered", functionName);

                string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(UpdateStandardsOrchestration), CancellationToken.None);

                _logger.LogInformation("{FunctionName} has started orchestration with {InstanceId}", functionName, instanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{FunctionName} has has failed", functionName);
            }
        }
    }
}
