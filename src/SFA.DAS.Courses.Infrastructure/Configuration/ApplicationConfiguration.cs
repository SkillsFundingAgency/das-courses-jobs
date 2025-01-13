namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    public class ApplicationConfiguration
    {
        public string GitHubRepositoryName { get; set; }
	    public string GitHubUserName { get; set; }
        public string GitHubEmail { get; set; }
	    public string GitHubAccessToken {  get; set; }

        public static string ApprenticeshipStandardsUrl => "https://www.instituteforapprenticeships.org/api/apprenticeshipstandards";
        public static string GitHubUrl => "https://api.github.com/repos/{0}/{1}/contents/";
    }
}
