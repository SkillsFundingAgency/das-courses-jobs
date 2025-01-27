using System.Text.Encodings.Web;
using System.Text.Json;
using SFA.DAS.Courses.Infrastructure.Api;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class ApprenticeshipStandardsService : IApprenticeshipStandardsService
    {
        private readonly HttpClient _client;
        private readonly ICoursesApi _coursesApi;

        public ApprenticeshipStandardsService(IHttpClientFactory httpClientFactory, ICoursesApi coursesApi)
        {
            _client = httpClientFactory.CreateClient("ifate");
            _coursesApi = coursesApi;
        }

        public async Task<Dictionary<string, string>> GetAllStandards()
        {
            var standardsImportUrl = await _coursesApi.GetStandardsImportUrl();
            _client.BaseAddress = new Uri(standardsImportUrl);

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