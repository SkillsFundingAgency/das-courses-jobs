using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.UnitTests.Services
{
    [TestFixture]
    public class ApprenticeshipStandardsServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IOptions<ApplicationConfiguration>> _optionsMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private ApplicationConfiguration _config;
        private ApprenticeshipStandardsService _sut;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("ifate"))
                .Returns(_httpClient);

            _optionsMock = new Mock<IOptions<ApplicationConfiguration>>();
            _config = new ApplicationConfiguration
            {
                SkillsEnglandApiConfiguration = new("http://standards.org/", "standards-path", "foundations-path", "apprenticeship-units-path")
            };
            _optionsMock.Setup(o => o.Value).Returns(_config);

            _sut = new ApprenticeshipStandardsService(_httpClientFactoryMock.Object, _optionsMock.Object);
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
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("standards-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponseData)
                });

            var mockResponseFoundationData = @"
            [
                {
                    ""referenceNumber"": ""FA0001"",
                    ""version"": ""1.0"",
                    ""title"": ""Test FA 1""
                },
                {
                    ""referenceNumber"": ""FA0002"",
                    ""version"": null,
                    ""title"": ""Another FA""
                }
            ]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("foundations-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponseFoundationData)
                });

            var mockResponseApprenticeshipUnitsData = @"
            [
                {
                    ""referenceNumber"": ""AU0001"",
                    ""version"": ""2.0"",
                    ""title"": ""Test Unit 1""
                },
                {
                    ""referenceNumber"": ""AU0002"",
                    ""version"": null,
                    ""title"": ""Another Unit""
                }
            ]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("apprenticeship-units-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponseApprenticeshipUnitsData)
                });

            // Act
            var result = await _sut.GetAllStandards();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result.Should().ContainKeys("ST0001_1.0", "ST0002_1.0", "FA0001_1.0", "FA0002_1.0", "AU0001_2.0", "AU0002_1.0");

            result["ST0001_1.0"].Should().Contain("Test Standard");
            result["ST0002_1.0"].Should().Contain("Another Standard");
            result["AU0001_2.0"].Should().Contain("Test Unit 1");
            result["AU0002_1.0"].Should().Contain("Another Unit");
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
        public async Task GetAllStandards_Should_Get_StandardsUrl_From_CoursesApi()
        {
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
            await _sut.GetAllStandards();

            // Assert
            _httpClient.BaseAddress.Should().Be("http://standards.org");
        }

        [Test]
        public async Task GetAllStandards_Should_Return_Dictionary_With_Default_Version_When_Version_Null()
        {
            // Arrange
            var standardsResponse = "[]";
            var foundationResponse = "[]";
            var unitsResponse = @"
            [
                {
                    ""referenceNumber"": ""AU0003"",
                    ""version"": null,
                    ""title"": ""Unit 3"",
                    ""overview"": ""Unit overview"",
                    ""typicalJobTitles"": [
                        {
                            ""name"": ""Job Title 1""
                        }
                    ]
                }
            ]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("standards-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(standardsResponse)
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("foundations-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(foundationResponse)
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("apprenticeship-units-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(unitsResponse)
                });

            // Act
            var result = await _sut.GetAllStandards();

            // Assert
            result.Should().ContainKey("AU0003_1.0");

            var unitDocument = JsonDocument.Parse(result["AU0003_1.0"]).RootElement;
            unitDocument.GetProperty("referenceNumber").GetString().Should().Be("AU0003");
            unitDocument.GetProperty("title").GetString().Should().Be("Unit 3");
            unitDocument.GetProperty("overview").GetString().Should().Be("Unit overview");
            unitDocument.GetProperty("typicalJobTitles")[0].GetProperty("name").GetString().Should().Be("Job Title 1");
        }

        [Test]
        public async Task GetAllStandards_Should_Return_Standards_And_Foundations_When_Units_Response_Is_Empty_Array()
        {
            // Arrange
            var standardsResponse = @"
            [
                {
                    ""referenceNumber"": ""ST1000"",
                    ""version"": ""1.1"",
                    ""title"": ""Standard 1000""
                }
            ]";
            var foundationResponse = @"
            [
                {
                    ""referenceNumber"": ""FA1000"",
                    ""version"": ""1.2"",
                    ""title"": ""Foundation 1000""
                }
            ]";
            var unitsResponse = "[]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("standards-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(standardsResponse)
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("foundations-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(foundationResponse)
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.AbsolutePath.Contains("apprenticeship-units-path")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(unitsResponse)
                });

            // Act
            var result = await _sut.GetAllStandards();

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainKeys("ST1000_1.1", "FA1000_1.2");
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
