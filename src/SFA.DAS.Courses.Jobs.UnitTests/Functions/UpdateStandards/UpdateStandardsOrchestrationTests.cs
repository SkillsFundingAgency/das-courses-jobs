using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.Functions.UpdateStandards;

namespace SFA.DAS.Courses.Jobs.UnitTests.Functions.UpdateStandards
{
    public class UpdateStandardsOrchestrationTests
    {
        private Mock<TaskOrchestrationContext> _mockContext;
        private Mock<ILogger> _mockLogger;
        private Dictionary<string, string> _testStandards;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<TaskOrchestrationContext>();
            _mockLogger = new Mock<ILogger>();

            _mockContext
                .Setup(c => c.CreateReplaySafeLogger(typeof(UpdateStandardsOrchestration)))
                .Returns(_mockLogger.Object);

            _testStandards = new Dictionary<string, string>
            {
                { "ST0001", "content1" },
                { "ST0002", "content2" }
            };
        }

        [Test]
        public async Task RunOrchestrator_AllStandardsSucceed_LogsSuccess()
        {
            // Arrange
            _mockContext
                .SetupSequence(c => c.CallActivityAsync<Dictionary<string, string>>(nameof(GetAllStandardsActivity), It.IsAny<TaskOptions>()))
                .ReturnsAsync(_testStandards);

            _mockContext
                .Setup(c => c.CallActivityAsync<List<string>>(nameof(StoreGitHubBatchActivity), It.IsAny<Dictionary<string, string>>(), null))
                .ReturnsAsync(new List<string>()); // no failures

            // Act
            await UpdateStandardsOrchestration.RunOrchestrator(_mockContext.Object);

            // Assert
            _mockContext.Verify(c => c.CallActivityAsync<List<string>>(
                nameof(StoreGitHubBatchActivity),
                It.Is<Dictionary<string, string>>(d => d.Count == 2),
                null), Times.Once);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("All standards processed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task RunOrchestrator_FailuresRetryUpToLimit()
        {
            // Arrange: Always return both keys as failed
            _mockContext
                .Setup(c => c.CallActivityAsync<Dictionary<string, string>>(nameof(GetAllStandardsActivity), It.IsAny<TaskOptions>()))
                .ReturnsAsync(_testStandards);

            _mockContext
                .Setup(c => c.CallActivityAsync<List<string>>(nameof(StoreGitHubBatchActivity), It.IsAny<Dictionary<string, string>>(), null))
                .ReturnsAsync(new List<string> { "ST0001", "ST0002" }); // all fail

            // Act
            await UpdateStandardsOrchestration.RunOrchestrator(_mockContext.Object);

            // Assert: activity retried 3 times
            _mockContext.Verify(c => c.CallActivityAsync<List<string>>(
                nameof(StoreGitHubBatchActivity),
                It.IsAny<Dictionary<string, string>>(),
                null), Times.Exactly(3));

            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Processing completed with 2 failures")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task RunOrchestrator_SomeSucceedOnRetry()
        {
            // Arrange
            var callCount = 0;

            _mockContext
                .Setup(c => c.CallActivityAsync<Dictionary<string, string>>(nameof(GetAllStandardsActivity), It.IsAny<TaskOptions>()))
                .ReturnsAsync(_testStandards);

            _mockContext
                .Setup(c => c.CallActivityAsync<List<string>>(nameof(StoreGitHubBatchActivity), It.IsAny<Dictionary<string, string>>(), null))
                .Returns(() =>
                {
                    callCount++;
                    return Task.FromResult(callCount == 1
                        ? new List<string> { "ST0002" }
                        : new List<string>()); // ST0002 fails once then succeeds
                });

            // Act
            await UpdateStandardsOrchestration.RunOrchestrator(_mockContext.Object);

            // Assert: retried only twice
            _mockContext.Verify(c => c.CallActivityAsync<List<string>>(
                nameof(StoreGitHubBatchActivity),
                It.IsAny<Dictionary<string, string>>(),
                null), Times.Exactly(2));

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("All standards processed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
