using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.Functions.UpdateStandards;
using SFA.DAS.Courses.Jobs.Services;
using SFA.DAS.Courses.Jobs.UnitTests.Extensions;

namespace SFA.DAS.Courses.Jobs.UnitTests.Functions.UpdateStandards
{
    [TestFixture]
    public class StoreGitHubActivityTests
    {
        private Mock<ILogger<StoreGitHubActivity>> _loggerMock;
        private Mock<IGitHubRepositoryService> _gitHubRepositoryServiceMock;
        private Mock<IApprenticeshipStandardsService> _apprenticeshipStandardsServiceMock;
        private StoreGitHubActivity _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<StoreGitHubActivity>>();
            _gitHubRepositoryServiceMock = new Mock<IGitHubRepositoryService>();
            _apprenticeshipStandardsServiceMock = new Mock<IApprenticeshipStandardsService>();

            _sut = new StoreGitHubActivity(
                _loggerMock.Object,
                _gitHubRepositoryServiceMock.Object,
                _apprenticeshipStandardsServiceMock.Object);
        }

        [Test]
        public async Task RunActivity_Should_Log_Start_And_End_Messages()
        {
            // Arrange
            _apprenticeshipStandardsServiceMock
                .Setup(s => s.GetAllStandards())
                .ReturnsAsync(new Dictionary<string, string>());

            // Act
            await _sut.RunActivity("test");

            // Assert
            _loggerMock.VerifyLogging(LogLevel.Information, "StoreGitHubActivity started at", Times.Once);
            _loggerMock.VerifyLogging(LogLevel.Information, "Found 0 standards", Times.Once);
        }

        [Test]
        public async Task RunActivity_Should_Process_All_Standards()
        {
            // Arrange
            var standards = new Dictionary<string, string>
            {
                { "ST0001_1.0", "Standard Content 1" },
                { "ST0002_1.0", "Standard Content 2" }
            };

            _apprenticeshipStandardsServiceMock
                .Setup(s => s.GetAllStandards())
                .ReturnsAsync(standards);

            _gitHubRepositoryServiceMock
                .Setup(s => s.GetFileInformation(It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(("sha123", "existing content"));

            // Act
            await _sut.RunActivity("test");

            // Assert
            _gitHubRepositoryServiceMock.Verify(
                s => s.GetFileInformation(It.IsAny<string>(), _loggerMock.Object),
                Times.Exactly(standards.Count));

            _gitHubRepositoryServiceMock.Verify(
                s => s.UpdateDocument(
                    It.IsAny<string>(),
                    It.IsAny<(string Sha, string Content)>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    _loggerMock.Object),
                Times.Exactly(standards.Count));
        }

        [Test]
        public async Task RunActivity_Should_Handle_Empty_Standards()
        {
            // Arrange
            _apprenticeshipStandardsServiceMock
                .Setup(s => s.GetAllStandards())
                .ReturnsAsync(new Dictionary<string, string>());

            // Act
            await _sut.RunActivity("test");

            // Assert
            _gitHubRepositoryServiceMock.Verify(
                s => s.GetFileInformation(It.IsAny<string>(), It.IsAny<ILogger>()),
                Times.Never);

            _gitHubRepositoryServiceMock.Verify(
                s => s.UpdateDocument(
                    It.IsAny<string>(),
                    It.IsAny<(string Sha, string Content)>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ILogger>()),
                Times.Never);
        }
    }

}
