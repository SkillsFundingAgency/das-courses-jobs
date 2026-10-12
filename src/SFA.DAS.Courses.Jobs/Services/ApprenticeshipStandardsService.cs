using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class ApprenticeshipStandardsService : IApprenticeshipStandardsService
    {
        private readonly HttpClient _client;
        private readonly SkillsEnglandApiConfiguration _apiConfiguration;
        private static JsonSerializerOptions JsonSerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public ApprenticeshipStandardsService(IHttpClientFactory httpClientFactory, IOptions<ApplicationConfiguration> options)
        {
            _client = httpClientFactory.CreateClient("ifate");
            _apiConfiguration = options.Value.SkillsEnglandApiConfiguration;
            _client.BaseAddress = new Uri(_apiConfiguration.ApiBaseUrl);
        }

        public async Task<Dictionary<string, string>> GetAllStandards()
        {
            var tasks = new[]
            {
                GetDocuments(_apiConfiguration.StandardsPath),
                GetDocuments(_apiConfiguration.FoundationApprenticeshipsPath),
                GetDocuments(_apiConfiguration.ApprenticeshipUnitsPath)
            };

            var results = await Task.WhenAll(tasks);

            return results
                .SelectMany(x => x)
                .ToDictionary();
        }

        private async Task<Dictionary<string, string>> GetDocuments(string path)
        {
            var response = await _client.GetAsync(path);
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
            return JsonSerializer.Serialize(element, JsonSerializerOptions);
        }
    }
}
