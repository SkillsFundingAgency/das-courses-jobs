using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApplicationConfiguration
    {
        public CoursesApiClientConfiguration CoursesApiClientConfiguration { get; set; }
        public string GitHubRepositoryName { get; set; }
        public string GitHubUserName { get; set; }
        public string GitHubEmail { get; set; }
        public string GitHubAccessToken {  get; set; }

        public static string GitHubUrl => "https://api.github.com/repos/{0}/{1}/contents/";
    }
}
