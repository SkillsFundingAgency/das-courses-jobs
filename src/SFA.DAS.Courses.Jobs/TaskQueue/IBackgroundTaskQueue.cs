using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.TaskQueue
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem, string taskName, Action<TimeSpan, ILogger<TaskQueueHostedService>> onComplete);

        Task<(Func<CancellationToken, Task> WorkItem, string TaskName, Action<TimeSpan, ILogger<TaskQueueHostedService>> OnComplete)> DequeueAsync(CancellationToken cancellationToken);
    }
}
