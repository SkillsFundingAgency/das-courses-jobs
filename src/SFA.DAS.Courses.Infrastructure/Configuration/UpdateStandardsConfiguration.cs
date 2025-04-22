using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class UpdateStandardsConfiguration
    {
        public GitHubConfiguration GitHubConfiguration { get; set; }
        public bool Enabled { get; set; }
    }
}
