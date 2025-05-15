using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class GitHubRepositoryService : IGitHubRepositoryService
    {
        private readonly HttpClient _gitHubContentsClient;
        private readonly GitHubConfiguration _gitHubConfiguration;

        public GitHubRepositoryService(IHttpClientFactory httpClientFactory,
            GitHubBearerTokenHolder bearerTokenHolder,
            IOptions<ApplicationConfiguration> options)
        {
            _gitHubContentsClient = httpClientFactory.CreateClient("github-contents");
            _gitHubContentsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerTokenHolder.BearerToken);
            _gitHubConfiguration = options.Value.FunctionsConfiguration.UpdateStandardsConfiguration.GitHubConfiguration;
        }

        public async Task UpdateDocument(string fileNamePrefix, (string? Sha, string? Content) existingFile, string updatedContent, string logProgress, ILogger log)
        {
            var fileName = GetFileName(fileNamePrefix);
            var request = new CreateFileRequest
            {
                Content = GetEncodedContent(updatedContent),
                Committer = new Committer { Name = _gitHubConfiguration.UserName, Email = _gitHubConfiguration.Email }
            };

            if (existingFile.Sha != null)
            {
                var existingContent = existingFile.Content != null ? UTF8Encoding.Default.GetString(Convert.FromBase64String(existingFile.Content)) : null;
                if (string.Compare(updatedContent, existingContent, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0)
                {
                    log.LogInformation("{Info} Skipping {FileName}", logProgress, fileName);
                    return;
                }
                request.Sha = existingFile.Sha;
                request.Message = $"Updating {fileName}";
            }
            else
            {
                request.Message = $"Adding {fileName}";
            }

            var response = await _gitHubContentsClient.PutAsync(fileName, new StringContent(JsonSerializer.Serialize(request)));

            if (!response.IsSuccessStatusCode)
            {
                var error = $"Error trying to update file - {response}";
                log.LogError(error);
                throw new GitHubFileException(error);
            }

            log.LogInformation("{Info} {RequestMessage}", logProgress, request.Message);
        }

        public async Task<(string? Sha, string? Content)> GetFileInformation(string fileNamePrefix, ILogger log)
        {
            var response = await _gitHubContentsClient.GetAsync(GetFileName(fileNamePrefix));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var rootElement = JsonDocument.Parse(result).RootElement;

                var sha = rootElement.GetProperty("sha").GetString();
                var content = rootElement.GetProperty("content").GetString();
                return (sha, content);
            }

            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = $"Error trying to get file information - {response}";
                log.LogError(error);
                throw new GitHubFileException(error);
            }

            return (null, null);
        }

        public static string GetFileName(string key)
        {
            return $"{key}.json";
        }

        private static string GetEncodedContent(string content)
        {
            return Convert.ToBase64String(UTF8Encoding.Default.GetBytes(content));
        }
    }

    public class CreateFileRequest
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("sha")]
        public string? Sha { get; set; }
        
        [JsonPropertyName("committer")]
        public Committer? Committer { get; set; }
    }

    public class Committer
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email {get; set;}
    }
}

