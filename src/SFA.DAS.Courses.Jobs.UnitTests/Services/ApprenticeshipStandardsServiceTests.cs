using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.UnitTests.Services
{
    [TestFixture]
    public class ApprenticeshipStandardsServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private ApprenticeshipStandardsService _sut;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("ifate"))
                .Returns(_httpClient);

            _sut = new ApprenticeshipStandardsService(_httpClientFactoryMock.Object);
        }

        [Test]
        public async Task GetAllStandards_Should_Return_Correct_Dictionary_When_Response_Is_Success()
        {
            // Arrange
            var mockResponseData = @"
            [
                {
                    ""referenceNumber"": ""ST0001"",
                    ""version"": ""1.0"",
                    ""title"": ""Test Standard""
                },
                {
                    ""referenceNumber"": ""ST0002"",
                    ""version"": null,
                    ""title"": ""Another Standard""
                }
            ]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponseData)
                });

            // Act
            var result = await _sut.GetAllStandards();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainKey("ST0001_1.0");
            result.Should().ContainKey("ST0002_1.0");

            result["ST0001_1.0"].Should().Contain("Test Standard");
            result["ST0002_1.0"].Should().Contain("Another Standard");
        }

        [Test]
        public async Task GetAllStandards_Should_Throw_If_Response_Is_Not_Success()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            Func<Task> act = _sut.GetAllStandards;

            // Assert
            await act.Should().ThrowAsync<JsonException>();
        }

        [Test]
        public async Task GetAllStandards_Should_Return_Empty_Dictionary_When_Response_Is_Empty_Array()
        {
            // Arrange
            var mockResponseData = "[]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponseData)
                });

            // Act
            var result = await _sut.GetAllStandards();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
