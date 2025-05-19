using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Functions.UpdateStandards;
using SFA.DAS.Courses.Jobs.Services;
using SFA.DAS.Courses.Jobs.TaskQueue;

namespace SFA.DAS.Courses.Jobs.UnitTests.Functions.UpdateStandards
{
    public class UpdateStandardsFunctionTests
    {
        private Mock<ILogger<UpdateStandardsFunction>> _mockLogger;
        private Mock<IApprenticeshipStandardsService> _mockStandardsService;
        private Mock<IGitHubRepositoryService> _mockGitHubService;
        private Mock<IBackgroundTaskQueue> _mockBackgroundTaskQueue;
        private ApplicationConfiguration _config;
        private UpdateStandardsFunction _sut;

        private readonly Dictionary<string, string> _testStandards = new()
        {
            { "ST0001", "content1" },
            { "ST0002", "content2" }
        };

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<UpdateStandardsFunction>>();
            _mockStandardsService = new Mock<IApprenticeshipStandardsService>();
            _mockGitHubService = new Mock<IGitHubRepositoryService>();
            _mockBackgroundTaskQueue = new Mock<IBackgroundTaskQueue>();

            _config = new ApplicationConfiguration
            {
                FunctionsConfiguration = new FunctionsConfiguration
                {
                    UpdateStandardsConfiguration = new UpdateStandardsConfiguration
                    {
                        Enabled = true
                    }
                }
            };

            _sut = new UpdateStandardsFunction(
                _config,
                _mockLogger.Object,
                _mockStandardsService.Object,
                _mockGitHubService.Object,
                _mockBackgroundTaskQueue.Object);
        }

        [Test]
        public async Task RunUpdateStandards_AllStandardsSucceed_ShouldLogSuccess()
        {
            // Arrange
            _mockStandardsService
                .Setup(x => x.GetAllStandards())
                .ReturnsAsync(_testStandards);

            _mockGitHubService
                .Setup(x => x.GetFileInformation(It.IsAny<string>()))
                .ReturnsAsync(("sha", "existing content"));

            _mockGitHubService
                .Setup(x => x.UpdateDocument(It.IsAny<string>(), It.IsAny<(string, string)>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RunUpdateStandards();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("completed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task RunUpdateStandards_SomeStandardsFailThenSucceed_ShouldRetry()
        {
            // Arrange
            var failFirstAttempt = true;

            _mockStandardsService
                .Setup(x => x.GetAllStandards())
                .ReturnsAsync(_testStandards);

            _mockGitHubService
                .Setup(x => x.GetFileInformation("ST0001"))
                .ReturnsAsync(("sha", "existing content"));

            _mockGitHubService
                .Setup(x => x.GetFileInformation("ST0002"))
                .Returns(() =>
                {
                    if (failFirstAttempt)
                    {
                        failFirstAttempt = false;
                        return Task.FromException<(string, string)>(new Exception("Temporary GitHub failure"));
                    }
                    return Task.FromResult(("sha", "existing content"));
                });

            _mockGitHubService
                .Setup(x => x.UpdateDocument(It.IsAny<string>(), It.IsAny<(string, string)>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RunUpdateStandards();

            // Assert
            _mockGitHubService.Verify(x => x.GetFileInformation("ST0002"), Times.Exactly(2));

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("completed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task RunUpdateStandards_AllFail_ShouldRetryThreeTimesAndLogWarning()
        {
            // Arrange
            _mockStandardsService
                .Setup(x => x.GetAllStandards())
                .ReturnsAsync(_testStandards);

            _mockGitHubService
                .Setup(x => x.GetFileInformation(It.IsAny<string>()))
                .ThrowsAsync(new Exception("GitHub unavailable"));

            // Act
            await _sut.RunUpdateStandards();

            // Assert
            _mockGitHubService.Verify(x => x.GetFileInformation(It.IsAny<string>()), Times.Exactly(6)); // 2 standards x 3 attempts

            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("failures after")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task RunUpdateStandards_Disabled_ShouldNotRun()
        {
            // Arrange
            _config.FunctionsConfiguration.UpdateStandardsConfiguration.Enabled = false;

            // Act
            await _sut.RunUpdateStandards();

            // Assert
            _mockStandardsService.Verify(x => x.GetAllStandards(), Times.Never);
            _mockGitHubService.Verify(x => x.UpdateDocument(It.IsAny<string>(), It.IsAny<(string, string)>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
