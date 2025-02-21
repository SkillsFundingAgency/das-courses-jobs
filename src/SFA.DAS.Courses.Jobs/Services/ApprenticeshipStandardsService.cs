using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SFA.DAS.Courses.Infrastructure.Api;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class ApprenticeshipStandardsService : IApprenticeshipStandardsService
    {
        private readonly HttpClient _client;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public ApprenticeshipStandardsService(IHttpClientFactory httpClientFactory, IOptions<ApplicationConfiguration> options)
        {
            _client = httpClientFactory.CreateClient("ifate");
            _applicationConfiguration = options.Value;
        }

        public async Task<Dictionary<string, string>> GetAllStandards()
        {
            _client.BaseAddress = new Uri(_applicationConfiguration.InstituteOfApprenticeshipsStandardsUrl);

            var response = await _client.GetAsync("");
            var result = await response.Content.ReadAsStringAsync();
            var rootElement = JsonDocument.Parse(result).RootElement;
            var documents = new Dictionary<string, string>();
            for (var i = 0; i < rootElement.GetArrayLength(); i++)
            {
                var standardElement = rootElement[i];
                var content = GetFormattedDocument(standardElement);
                var standardReference = standardElement.GetProperty("referenceNumber").GetString();
                var version = standardElement.GetProperty("version").GetString();
                if (string.IsNullOrWhiteSpace(version)) version = "1.0";
                documents.Add($"{standardReference}_{version}", content);
            }
            return documents;
        }

        private static string GetFormattedDocument(JsonElement element)
        {
            return JsonSerializer.Serialize(element, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
        }
    }
}