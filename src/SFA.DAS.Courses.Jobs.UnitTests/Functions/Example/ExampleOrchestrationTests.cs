using System;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Courses.Jobs.Functions.Example.UnitTests
{
    public class ExampleOrchestrationTests
    {
        [Test, MoqAutoData]
        public async Task Orchestrator_Should_Call_ExampleActivity()
        {
            // Arrange
            var mockContext = new Mock<TaskOrchestrationContext>();

            mockContext
                .Setup(x => x.CreateReplaySafeLogger(It.Is<Type>(p => p == typeof(ExampleOrchestration))))
                .Returns(new Mock<ILogger>().Object);

            // Act
            await ExampleOrchestration.RunOrchestrator(mockContext.Object);

            // Assert
            mockContext
                .Verify(x => x.CallActivityAsync(nameof(ExampleActivity), null, null), Times.Once);
        }
    }
}
