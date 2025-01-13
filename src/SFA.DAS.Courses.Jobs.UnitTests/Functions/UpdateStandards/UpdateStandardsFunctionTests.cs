using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.UnitTests.Helpers;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards.UnitTests
{
    public class UpdateStandardsFunctionTests
    {
        [Test]
        public async Task RunTimerTrigger_Should_Start_New_Orchestration_And_Log_Messages()
        {
            // Arrange
            var mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            var mockLogger = new Mock<ILogger<UpdateStandardsFunction>>();
            var timerInfo = new TimerInfo();

            var function = new UpdateStandardsFunction(mockLogger.Object);

            mockDurableTaskClient
                .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(nameof(UpdateStandardsOrchestration), CancellationToken.None))
                .ReturnsAsync("test-instance-id");

            // Act
            await function.UpdateStandardsTimer(timerInfo, mockDurableTaskClient.Object);

            // Assert
            mockDurableTaskClient.Verify(x =>
                x.ScheduleNewOrchestrationInstanceAsync(nameof(UpdateStandardsOrchestration), CancellationToken.None),
                Times.Once);
        }
    }
}
