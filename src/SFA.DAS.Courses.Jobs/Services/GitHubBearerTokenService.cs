using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Services
{
    public class GitHubBearerTokenService
    {
        private readonly string _keyVaultIdentifier;
        private readonly string _keyVaultSecretName;

        public GitHubBearerTokenService(ApplicationConfiguration configuration)
        {
            _keyVaultIdentifier = configuration.GitHubConfiguration.AccessTokenConfiguration.KeyVaultIdentifier;
            _keyVaultSecretName = configuration.GitHubConfiguration.AccessTokenConfiguration.KeyVaultSecretName;
        }

        public async Task<string> GetSecret()
        {
            var client = new SecretClient(new Uri($"https://{_keyVaultIdentifier}.vault.azure.net/"), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(_keyVaultSecretName);
            return secret.Value.Value;
        }
    }
}