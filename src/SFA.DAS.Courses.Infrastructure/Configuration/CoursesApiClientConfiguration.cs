using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class CoursesApiClientConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string IdentifierUri { get; set; }

        public string Version { get; set; }
    }
}