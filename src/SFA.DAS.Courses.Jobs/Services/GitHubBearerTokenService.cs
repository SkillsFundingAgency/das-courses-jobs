using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Exceptions;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class GitHubBearerTokenService : IHostedService
    {
        private readonly string _keyVaultSecretName;
        private readonly string? _environmentName;
        private readonly string? _localToken;
        private readonly GitHubBearerTokenHolder _bearerTokenHolder;
        private readonly ISecretClient _secretClient;
        private readonly ILogger<GitHubBearerTokenService> _logger;
        
        public GitHubBearerTokenService(
            ApplicationConfiguration configuration,
            GitHubBearerTokenHolder bearerTokenHolder,
            ISecretClient secretClient,
            ILogger<GitHubBearerTokenService> logger,
            IConfiguration config)
        {
            var gitHubAccessTokenConfiguration = configuration.FunctionsConfiguration.UpdateStandardsConfiguration.GitHubConfiguration.AccessTokenConfiguration;
            _keyVaultSecretName = gitHubAccessTokenConfiguration.KeyVaultSecretName;
            _bearerTokenHolder = bearerTokenHolder;
            _secretClient = secretClient;
            _logger = logger;
            
            _environmentName = config["EnvironmentName"];
            _localToken = config[_keyVaultSecretName];
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_environmentName?.Equals("LOCAL", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    if (!string.IsNullOrEmpty(_localToken))
                    {
                        _bearerTokenHolder.BearerToken = _localToken;
                        _logger.LogInformation("Retrieved GitHub bearer token from AppSettings");
                    }
                    else
                    {
                        _logger.LogWarning("Unable to get GitHub bearer token from AppSettings");
                    }
                }
                else
                {
                    var keyvaultToken = await _secretClient.GetSecretAsync(_keyVaultSecretName, cancellationToken);
                    _bearerTokenHolder.BearerToken = keyvaultToken;   
                    _logger.LogInformation("Retrieved GitHub bearer token from Keyvault");
                }
            }
            catch (KeyvaultAccessException ex)
            {
                _logger.LogCritical(ex, "Unable to get GitHub bearer token from Keyvault");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
