using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class GitHubBearerTokenService : IHostedService
    {
        private readonly string _keyVaultIdentifier;
        private readonly string _keyVaultSecretName;

        private readonly GitHubBearerTokenHolder _bearerTokenHolder;
        private readonly ILogger<GitHubBearerTokenService> _logger;

        public GitHubBearerTokenService(
            ApplicationConfiguration configuration,
            GitHubBearerTokenHolder bearerTokenHolder,
            ILogger<GitHubBearerTokenService> logger)
        {
            _keyVaultIdentifier = configuration.GitHubConfiguration.AccessTokenConfiguration.KeyVaultIdentifier;
            _keyVaultSecretName = configuration.GitHubConfiguration.AccessTokenConfiguration.KeyVaultSecretName;
            _bearerTokenHolder = bearerTokenHolder;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = new SecretClient(new Uri($"https://{_keyVaultIdentifier}.vault.azure.net/"), new DefaultAzureCredential());
                var secret = await client.GetSecretAsync(_keyVaultSecretName);
                _bearerTokenHolder.BearerToken = secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unable to get GitHub bearer token from {KeyVaultIdentifier} using {KeyVaultSecretName}", _keyVaultIdentifier, _keyVaultSecretName);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
