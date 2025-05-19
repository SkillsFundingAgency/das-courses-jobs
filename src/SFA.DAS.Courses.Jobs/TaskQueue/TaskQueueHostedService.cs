using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.TaskQueue
{
    public class TaskQueueHostedService : BackgroundService
    {
        private readonly ILogger<TaskQueueHostedService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public TaskQueueHostedService(
            IBackgroundTaskQueue taskQueue,
            ILogger<TaskQueueHostedService> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Queue Hosted Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var (workItem, taskName, onComplete) = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    await workItem(stoppingToken);
                    stopwatch.Stop();

                    onComplete?.Invoke(stopwatch.Elapsed, _logger);
                }
                catch (OperationCanceledException)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in background task {TaskName}.", taskName);
                }
            }

            _logger.LogInformation("Task Queue Hosted Service is stopping.");
        }
    }
}
