using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.UnitTests.Services
{
    public class GitHubBearerTokenServiceTests
    {
        private Mock<ISecretClient> _mockSecretClient;
        private Mock<ILogger<GitHubBearerTokenService>> _mockLogger;
        private Mock<IConfiguration> _mockConfig;
        private GitHubBearerTokenHolder _tokenHolder;

        private const string SecretName = "my-secret";
        private ApplicationConfiguration _appConfig;

        [SetUp]
        public void Setup()
        {
            _mockSecretClient = new Mock<ISecretClient>();
            _mockLogger = new Mock<ILogger<GitHubBearerTokenService>>();
            _mockConfig = new Mock<IConfiguration>();
            _tokenHolder = new GitHubBearerTokenHolder();

            _appConfig = new ApplicationConfiguration
            {
                FunctionsConfiguration = new FunctionsConfiguration
                {
                    UpdateStandardsConfiguration = new UpdateStandardsConfiguration
                    {
                        GitHubConfiguration = new GitHubConfiguration
                        {
                            AccessTokenConfiguration = new GitHubAccessTokenConfiguration
                            {
                                KeyVaultSecretName = SecretName
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public async Task StartAsync_WhenEnvironmentIsLocalAndTokenExists_ShouldSetTokenFromConfig()
        {
            // Arrange
            _mockConfig.Setup(m => m["EnvironmentName"]).Returns("LOCAL");
            _mockConfig.Setup(m => m[SecretName]).Returns("local-token");

            var service = new GitHubBearerTokenService(_appConfig, _tokenHolder, _mockSecretClient.Object, _mockLogger.Object, _mockConfig.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            // Assert
            Assert.That(_tokenHolder.BearerToken, Is.EqualTo("local-token"));
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Retrieved GitHub bearer token from AppSettings")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task StartAsync_WhenEnvironmentIsLocalAndTokenMissing_ShouldLogWarning()
        {
            // Arrange
            _mockConfig.Setup(m => m["EnvironmentName"]).Returns("LOCAL");
            _mockConfig.Setup(m => m[SecretName]).Returns((string)null);

            var service = new GitHubBearerTokenService(_appConfig, _tokenHolder, _mockSecretClient.Object, _mockLogger.Object, _mockConfig.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            // Assert
            Assert.That(_tokenHolder.BearerToken, Is.Null);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Unable to get GitHub bearer token from AppSettings")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task StartAsync_WhenEnvironmentIsNotLocal_ShouldRetrieveFromKeyVault()
        {
            // Arrange
            _mockConfig.Setup(m => m["EnvironmentName"]).Returns("PROD");
            _mockConfig.Setup(m => m[SecretName]).Returns((string)null);
            _mockSecretClient.Setup(x => x.GetSecretAsync(SecretName, It.IsAny<CancellationToken>())).ReturnsAsync("vault-token");

            var service = new GitHubBearerTokenService(_appConfig, _tokenHolder, _mockSecretClient.Object, _mockLogger.Object, _mockConfig.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            // Assert
            Assert.That(_tokenHolder.BearerToken, Is.EqualTo("vault-token"));
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Retrieved GitHub bearer token from Keyvault")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task StartAsync_WhenKeyVaultThrowsException_ShouldLogCritical()
        {
            // Arrange
            _mockConfig.Setup(m => m["EnvironmentName"]).Returns("PROD");
            _mockConfig.Setup(m => m[SecretName]).Returns((string)null);
            var ex = new KeyvaultAccessException("boom");
            _mockSecretClient.Setup(x => x.GetSecretAsync(SecretName, It.IsAny<CancellationToken>())).ThrowsAsync(ex);

            var service = new GitHubBearerTokenService(_appConfig, _tokenHolder, _mockSecretClient.Object, _mockLogger.Object, _mockConfig.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Unable to get GitHub bearer token from Keyvault")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
