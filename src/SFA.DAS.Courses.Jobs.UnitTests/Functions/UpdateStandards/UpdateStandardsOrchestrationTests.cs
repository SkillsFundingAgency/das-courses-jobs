using System;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards.UnitTests
{
    public class UpdateStandardsOrchestrationTests
    {
        [Test]
        public async Task Orchestrator_Should_Call_StoreGitHubActivity()
        {
            // Arrange
            var mockContext = new Mock<TaskOrchestrationContext>();

            mockContext
                .Setup(x => x.CreateReplaySafeLogger(It.Is<Type>(p => p == typeof(UpdateStandardsOrchestration))))
                .Returns(new Mock<ILogger>().Object);

            // Act
            await UpdateStandardsOrchestration.RunOrchestrator(mockContext.Object);

            // Assert
            mockContext.Verify(x => x.CallActivityAsync(nameof(StoreGitHubActivity),null, 
                It.Is<TaskOptions>(options =>
                    options.Retry != null &&
                    options.Retry.Policy.MaxNumberOfAttempts == 3 &&
                    options.Retry.Policy.FirstRetryInterval == TimeSpan.FromMinutes(10))), 
                Times.Once);
        }
    }
}
