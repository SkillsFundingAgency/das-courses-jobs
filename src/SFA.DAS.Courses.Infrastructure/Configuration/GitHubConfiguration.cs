using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class GitHubConfiguration
    {
        public string RepositoryName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public GitHubAccessTokenConfiguration AccessTokenConfiguration { get; set; }
        public static string GitHubUrl => "https://api.github.com/repos/{0}/contents/";
    }
}
