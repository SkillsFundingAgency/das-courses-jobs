using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class GitHubAccessTokenConfiguration
    {
        public string KeyVaultIdentifier { get; set; }
        public string KeyVaultSecretName { get; set; }
    }
}
