using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.TaskQueue
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<(Func<CancellationToken, Task> WorkItem, string TaskName, Action<TimeSpan, ILogger<TaskQueueHostedService>> OnComplete)> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);
        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem, string taskName, Action<TimeSpan, ILogger<TaskQueueHostedService>> onComplete)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            _queue.Enqueue((workItem, taskName, onComplete));
            _signal.Release();
        }

        public async Task<(Func<CancellationToken, Task> WorkItem, string TaskName, Action<TimeSpan, ILogger<TaskQueueHostedService>> OnComplete)> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _queue.TryDequeue(out var item);
            return item;
        }
    }
}
