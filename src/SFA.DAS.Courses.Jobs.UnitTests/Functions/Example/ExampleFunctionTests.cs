using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.Functions.Example;
using SFA.DAS.Courses.Jobs.UnitTests.Helpers;

namespace SFA.DAS.Courses.Jobs.Functions.Example.UnitTests
{
    public class ExampleFunctionTests
    {
        [Test]
        public async Task RunTimerTrigger_Should_Start_New_Orchestration_And_Log_Messages()
        {
            // Arrange
            var mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            var mockLogger = new Mock<ILogger<ExampleFunction>>();
            var timerInfo = new TimerInfo();

            var function = new ExampleFunction(mockLogger.Object);

            mockDurableTaskClient
                .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(nameof(ExampleOrchestration), CancellationToken.None))
                .ReturnsAsync("test-instance-id");

            // Act
            await function.ExampleTimer(timerInfo, mockDurableTaskClient.Object);

            // Assert
            mockDurableTaskClient.Verify(x =>
                x.ScheduleNewOrchestrationInstanceAsync(nameof(ExampleOrchestration), CancellationToken.None),
                Times.Once);
        }
    }
}
