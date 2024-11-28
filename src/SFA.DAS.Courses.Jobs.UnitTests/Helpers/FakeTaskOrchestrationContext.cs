using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Courses.Jobs.UnitTests.Helpers
{
    public class FakeTaskOrchestrationContext : TaskOrchestrationContext
    {
        public override TaskName Name => new TaskName("AzureActivityFunction");

        public override string InstanceId => "activityInstanceId";

        protected override ILoggerFactory LoggerFactory => new NullLoggerFactory();

        public override Task<TResult> CallActivityAsync<TResult>(TaskName name, object input = null, TaskOptions options = null)
        {
            if (typeof(TResult) == typeof(string))
            {
                return Task.FromResult((TResult)(object)InstanceId);
            }
            else
            {
                return Task.FromResult(default(TResult));
            }
        }

        public override ParentOrchestrationInstance Parent => throw new NotImplementedException();

        public override DateTime CurrentUtcDateTime => DateTime.UtcNow;

        public override bool IsReplaying => false;

        public override Task<TResult> CallSubOrchestratorAsync<TResult>(TaskName orchestratorName, object input = null, TaskOptions options = null)
        {
            throw new NotImplementedException();
        }

        public override void ContinueAsNew(object newInput = null, bool preserveUnprocessedEvents = true)
        {
            throw new NotImplementedException();
        }

        public override Task CreateTimer(DateTime fireAt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override T GetInput<T>() where T : default
        {
            throw new NotImplementedException();
        }

        public override Guid NewGuid()
        {
            throw new NotImplementedException();
        }

        public override void SendEvent(string instanceId, string eventName, object payload)
        {
            throw new NotImplementedException();
        }

        public override void SetCustomStatus(object customStatus)
        {
            throw new NotImplementedException();
        }

        public override Task<T> WaitForExternalEvent<T>(string eventName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override ILogger CreateReplaySafeLogger<T>()
        {
            return new Mock<ILogger<T>>() as ILogger;
        }
    }
}