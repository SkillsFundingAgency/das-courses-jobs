using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.Functions.UpdateStandards;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.UnitTests.Functions.UpdateStandards
{
    public class StoreGitHubBatchActivityTests
    {
        private Mock<ILogger<StoreGitHubBatchActivity>> _mockLogger;
        private Mock<IGitHubRepositoryService> _mockGitHubRepositoryService;
        private StoreGitHubBatchActivity _activity;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<StoreGitHubBatchActivity>>();
            _mockGitHubRepositoryService = new Mock<IGitHubRepositoryService>();

            _activity = new StoreGitHubBatchActivity(
                _mockLogger.Object,
                _mockGitHubRepositoryService.Object
            );
        }

        [Test]
        public async Task Run_AllStandardsSucceed_ReturnsEmptyList()
        {
            // Arrange
            var standards = new Dictionary<string, string>
            {
                { "ST0001", "content1" },
                { "ST0002", "content2" }
            };

            _mockGitHubRepositoryService
                .Setup(s => s.GetFileInformation(It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(("sha123", "existing content"));

            _mockGitHubRepositoryService
                .Setup(x => x.UpdateDocument(
                    It.IsAny<string>(), It.IsAny<(string, string)>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .Returns(Task.CompletedTask);

            // Act
            var failed = await _activity.Run(standards);

            // Assert
            failed.Should().BeEmpty();
        }

        [Test]
        public async Task Run_OneStandardFails_ReturnsFailedKey()
        {
            // Arrange
            var standards = new Dictionary<string, string>
            {
                { "ST0001", "content1" },
                { "ST0002", "content2" }
            };

            _mockGitHubRepositoryService
                .Setup(x => x.GetFileInformation("ST0001", It.IsAny<ILogger>()))
                .ReturnsAsync(("sha123", "existing content"));

            _mockGitHubRepositoryService
                .Setup(x => x.GetFileInformation("ST0002", It.IsAny<ILogger>()))
                .ThrowsAsync(new Exception("GitHub error"));

            // Act
            var failed = await _activity.Run(standards);

            // Assert
            failed.Should().BeEquivalentTo("ST0002");
        }

        [Test]
        public async Task Run_AllStandardsFail_ReturnsAllKeys()
        {
            // Arrange
            var standards = new Dictionary<string, string>
            {
                { "ST0001", "content1" },
                { "ST0002", "content2" }
            };

            _mockGitHubRepositoryService
                .Setup(x => x.GetFileInformation(It.IsAny<string>(), It.IsAny<ILogger>()))
                .ThrowsAsync(new Exception("API down"));

            // Act
            var failed = await _activity.Run(standards);

            // Assert
            failed.Should().BeEquivalentTo("ST0001", "ST0002");
        }
    }
}
