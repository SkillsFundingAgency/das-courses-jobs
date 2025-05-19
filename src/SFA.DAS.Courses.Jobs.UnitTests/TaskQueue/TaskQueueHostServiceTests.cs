using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.TaskQueue;

namespace SFA.DAS.Courses.Jobs.UnitTests.TaskQueue
{
    public class TaskQueueHostedServiceTests
    {
        private Mock<IBackgroundTaskQueue> _mockQueue;
        private Mock<ILogger<TaskQueueHostedService>> _mockLogger;
        private TaskQueueHostedService _sut;
        private CancellationTokenSource _cts;

        [SetUp]
        public void Setup()
        {
            _mockQueue = new Mock<IBackgroundTaskQueue>();
            _mockLogger = new Mock<ILogger<TaskQueueHostedService>>();
            _cts = new CancellationTokenSource();
            _sut = new TaskQueueHostedService(_mockQueue.Object, _mockLogger.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _cts.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_DequeuesAndExecutesWorkItem_AndCallsOnComplete()
        {
            // Arrange
            var onCompleteSignal = new TaskCompletionSource();

            Func<CancellationToken, Task> workItem = async ct =>
            {
                await Task.Delay(10, ct);
            };

            string taskName = "TestTask";
            Action<TimeSpan, ILogger<TaskQueueHostedService>> onComplete = (duration, logger) =>
            {
                logger.LogInformation("Task finished in {Duration}", duration);
                onCompleteSignal.SetResult(); // signal that completion happened
            };

            _mockQueue.Setup(q => q.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((workItem, taskName, onComplete));

            // Act
            var runTask = _sut.StartAsync(_cts.Token);
            await onCompleteSignal.Task;

            await _cts.CancelAsync(); // cancel the background task loop
            await runTask;

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Task Queue Hosted Service is starting.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Task finished")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }



        [Test]
        public async Task ExecuteAsync_WhenWorkItemThrows_LogsError()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");

            Func<CancellationToken, Task> workItem = ct => throw exception;
            string taskName = "FailingTask";
            Action<TimeSpan, ILogger<TaskQueueHostedService>> onComplete = (_, _) => { };

            _mockQueue.Setup(q => q.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((workItem, taskName, onComplete))
                .Callback(() => _cts.Cancel());

            // Act
            await _sut.StartAsync(_cts.Token);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Error occurred in background task")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenCancelled_LogsStoppingMessage()
        {
            // Arrange
            await _cts.CancelAsync();

            // Act
            await _sut.StartAsync(_cts.Token);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Task Queue Hosted Service is stopping.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
