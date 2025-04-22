using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SFA.DAS.Courses.Jobs.Exceptions;

namespace SFA.DAS.Courses.Jobs.Services
{
    [ExcludeFromCodeCoverage]
    public class AzureSecretClient : ISecretClient
    {
        private readonly SecretClient _client;

        public AzureSecretClient(string keyVaultIdentifier)
        {
            _client = new SecretClient(
                new Uri($"https://{keyVaultIdentifier}.vault.azure.net/"),
                new DefaultAzureCredential());
           
        }

        public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken)
        {
            try
            {
                var secret = await _client.GetSecretAsync(secretName, null, cancellationToken);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                throw new KeyvaultAccessException($"Unable to get secret from {_client.VaultUri} using {secretName}", ex);
            }
        }
    }
}
