using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.UnitTests.Services
{
    namespace SFA.DAS.Courses.Jobs.UnitTests.Services
    {
        public class GitHubBearerTokenServiceTests
        {
            private Mock<ISecretClient> _mockSecretClient;
            private Mock<ILogger<GitHubBearerTokenService>> _mockLogger;
            private GitHubBearerTokenHolder _tokenHolder;
            private GitHubBearerTokenService _service;

            [SetUp]
            public void Setup()
            {
                _mockSecretClient = new Mock<ISecretClient>();
                _mockLogger = new Mock<ILogger<GitHubBearerTokenService>>();
                _tokenHolder = new GitHubBearerTokenHolder();

                var config = new ApplicationConfiguration
                {
                    FunctionsConfiguration = new FunctionsConfiguration
                    {
                        UpdateStandardsConfiguration = new UpdateStandardsConfiguration
                        {
                            GitHubConfiguration = new GitHubConfiguration
                            {
                                AccessTokenConfiguration = new GitHubAccessTokenConfiguration
                                {
                                    KeyVaultSecretName = "my-secret"
                                }
                            }
                        }
                    }
                };

                _service = new GitHubBearerTokenService(config, _tokenHolder, _mockSecretClient.Object, _mockLogger.Object);
            }

            [Test]
            public async Task StartAsync_Should_SetBearerToken_WhenSecretRetrievedSuccessfully()
            {
                // Arrange
                _mockSecretClient
                    .Setup(x => x.GetSecretAsync("my-secret", It.IsAny<CancellationToken>()))
                    .ReturnsAsync("test-token");

                // Act
                await _service.StartAsync(CancellationToken.None);

                // Assert
                Assert.That(_tokenHolder.BearerToken, Is.EqualTo("test-token"));
            }

            [Test]
            public async Task StartAsync_Should_LogCritical_WhenKeyVaultAccessExceptionThrown()
            {
                // Arrange
                var exception = new KeyvaultAccessException("Key Vault unavailable");

                _mockSecretClient
                    .Setup(x => x.GetSecretAsync("my-secret", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

                // Act
                await _service.StartAsync(CancellationToken.None);

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Critical,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get GitHub bearer token")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }

}