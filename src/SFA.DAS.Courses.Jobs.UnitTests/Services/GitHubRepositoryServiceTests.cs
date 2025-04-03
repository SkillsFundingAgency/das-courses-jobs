using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;
using SFA.DAS.Courses.Jobs.Services;
using SFA.DAS.Courses.Jobs.UnitTests.Extensions;

namespace SFA.DAS.Courses.Jobs.UnitTests.Services
{
    [TestFixture]
    public class GitHubRepositoryServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IOptions<ApplicationConfiguration>> _optionsMock;
        private Mock<ILogger> _loggerMock;
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private GitHubRepositoryService _sut;
        private ApplicationConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _optionsMock = new Mock<IOptions<ApplicationConfiguration>>();
            _loggerMock = new Mock<ILogger>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClient.BaseAddress = new Uri("https://github.com/");

            _httpClientFactoryMock
                .Setup(x => x.CreateClient("github-contents"))
                .Returns(_httpClient);

            _config = new ApplicationConfiguration
            {
                GitHubConfiguration = new GitHubConfiguration
                {
                    UserName = "TestUser",
                    Email = "testuser@example.com"
                }
            };
            _optionsMock.Setup(o => o.Value).Returns(_config);

            _sut = new GitHubRepositoryService(_httpClientFactoryMock.Object, new GitHubBearerTokenHolder(), _optionsMock.Object);
        }

        [Test]
        public async Task UpdateDocument_Should_Log_Skip_If_Content_Is_Unchanged()
        {
            // Arrange
            var existingFile = ("sha123", Convert.ToBase64String(Encoding.UTF8.GetBytes("existing content")));
            var updatedContent = "existing content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            // Act
            await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyLogging(LogLevel.Information, $"{logProgress} Skipping {fileNamePrefix}.json", Times.Once);
        }

        [Test]
        public async Task UpdateDocument_Should_Log_Update_If_Content_Is_Changed()
        {
            // Arrange
            var existingFile = ("sha123", Convert.ToBase64String(Encoding.UTF8.GetBytes("new content")));
            var updatedContent = "existing content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), // Match any HttpRequestMessage
                    ItExpr.IsAny<CancellationToken>()) // Match any CancellationToken
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"key\":\"value\"}")
                });

            // Act
            await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyLogging(LogLevel.Information, $"{logProgress} Updating {fileNamePrefix}.json", Times.Once);
        }

        [Test]
        public async Task UpdateDocument_Should_Call_GitHub_If_Content_Is_Changed()
        {
            // Arrange
            var existingFile = ("sha123", Convert.ToBase64String(Encoding.UTF8.GetBytes("new content")));
            var updatedContent = "existing content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            HttpRequestMessage capturedRequest = null;

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"key\":\"value\"}")
                });

            // Act
            await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri == new Uri($"https://github.com/{fileNamePrefix}.json")),
                ItExpr.IsAny<CancellationToken>());

            // Verify the content
            capturedRequest.Should().NotBeNull();
            var requestBody = await capturedRequest.Content.ReadAsStringAsync();
            requestBody.Should().NotBeNullOrEmpty();

            var deserializedRequest = JsonSerializer.Deserialize<CreateFileRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            deserializedRequest.Should().NotBeNull();
            deserializedRequest.Content.Should().Be(Convert.ToBase64String(UTF8Encoding.Default.GetBytes(updatedContent)));
            deserializedRequest.Message.Should().Be($"Updating {fileNamePrefix}.json");
            deserializedRequest.Committer.Should().NotBeNull();
            deserializedRequest.Committer.Name.Should().Be(_config.GitHubConfiguration.UserName);
            deserializedRequest.Committer.Email.Should().Be(_config.GitHubConfiguration.Email);
        }

        [Test]
        public async Task UpdateDocument_Should_Log_Adding_If_File_Is_New()
        {
            // Arrange
            var existingFile = (null as string, null as string);
            var updatedContent = "new content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), // Match any HttpRequestMessage
                    ItExpr.IsAny<CancellationToken>()) // Match any CancellationToken
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"key\":\"value\"}")
                });

            // Act
            await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyLogging(LogLevel.Information, $"{logProgress} Adding {fileNamePrefix}.json", Times.Once);
        }

        [Test]
        public async Task UpdateDocument_Should_Call_GitHub_If_Content_Is_Added()
        {
            // Arrange
            var existingFile = (null as string, null as string);
            var updatedContent = "new content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            HttpRequestMessage capturedRequest = null;

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"key\":\"value\"}")
                });

            // Act
            await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri == new Uri($"https://github.com/{fileNamePrefix}.json")),
                ItExpr.IsAny<CancellationToken>());

            // Verify the content
            capturedRequest.Should().NotBeNull();
            var requestBody = await capturedRequest.Content.ReadAsStringAsync();
            requestBody.Should().NotBeNullOrEmpty();

            var deserializedRequest = JsonSerializer.Deserialize<CreateFileRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            deserializedRequest.Should().NotBeNull();
            deserializedRequest.Content.Should().Be(Convert.ToBase64String(UTF8Encoding.Default.GetBytes(updatedContent)));
            deserializedRequest.Message.Should().Be($"Adding {fileNamePrefix}.json");
            deserializedRequest.Committer.Should().NotBeNull();
            deserializedRequest.Committer.Name.Should().Be(_config.GitHubConfiguration.UserName);
            deserializedRequest.Committer.Email.Should().Be(_config.GitHubConfiguration.Email);
        }
    
        [Test]
        public async Task UpdateDocument_Should_Throw_Exception_When_Response_Is_Not_Success()
        {
            // Arrange
            var existingFile = ("sha123", null as string);
            var updatedContent = "new content";
            var fileNamePrefix = "test";
            var logProgress = "Progress";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            Func<Task> act = async () => await _sut.UpdateDocument(fileNamePrefix, existingFile, updatedContent, logProgress, _loggerMock.Object);

            // Assert
            await act.Should().ThrowAsync<GitHubFileException>().WithMessage("Error trying to update file*");
            _loggerMock.VerifyLogging(LogLevel.Error, $"Error trying to update file", Times.Once);
        }

        [Test]
        public async Task GetFileInformation_Should_Return_File_Info_When_Response_Is_Success()
        {
            // Arrange
            var fileNamePrefix = "test";
            var responseData = "{\"sha\":\"sha123\",\"content\":\"" + Convert.ToBase64String(Encoding.UTF8.GetBytes("content")) + "\"}";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseData)
                });

            // Act
            var result = await _sut.GetFileInformation(fileNamePrefix, _loggerMock.Object);

            // Assert
            result.Sha.Should().Be("sha123");
            result.Content.Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes("content")));
        }

        [Test]
        public async Task GetFileInformation_Should_Throw_Exception_When_Response_Is_Not_Success_And_Not_NotFound()
        {
            // Arrange
            var fileNamePrefix = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            Func<Task> act = async () => await _sut.GetFileInformation(fileNamePrefix, _loggerMock.Object);

            // Assert
            await act.Should().ThrowAsync<GitHubFileException>().WithMessage("Error trying to get file information*");
            _loggerMock.VerifyLogging(LogLevel.Error, "Error trying to get file information", Times.Once);
        }

        [Test]
        public async Task GetFileInformation_Should_Return_Null_When_Response_Is_NotFound()
        {
            // Arrange
            var fileNamePrefix = "test";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _sut.GetFileInformation(fileNamePrefix, _loggerMock.Object);

            // Assert
            result.Sha.Should().BeNull();
            result.Content.Should().BeNull();
        }
    }
}
