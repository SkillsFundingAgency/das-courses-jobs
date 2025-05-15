using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class GitHubBearerTokenService : IHostedService
    {
        private readonly string _keyVaultSecretName;

        private readonly GitHubBearerTokenHolder _bearerTokenHolder;
        private readonly ISecretClient _secretClient;
        private readonly ILogger<GitHubBearerTokenService> _logger;

        public GitHubBearerTokenService(
            ApplicationConfiguration configuration,
            GitHubBearerTokenHolder bearerTokenHolder,
            ISecretClient secretClient,
            ILogger<GitHubBearerTokenService> logger)
        {
            var gitHubAccessTokenConfiguration = configuration.FunctionsConfiguration.UpdateStandardsConfiguration.GitHubConfiguration.AccessTokenConfiguration;
            _keyVaultSecretName = gitHubAccessTokenConfiguration.KeyVaultSecretName;
            _bearerTokenHolder = bearerTokenHolder;
            _secretClient = secretClient;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(_keyVaultSecretName, cancellationToken);
                _bearerTokenHolder.BearerToken = secret;
            }
            catch (KeyvaultAccessException ex)
            {
                _logger.LogCritical(ex, "Unable to get GitHub bearer token");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
