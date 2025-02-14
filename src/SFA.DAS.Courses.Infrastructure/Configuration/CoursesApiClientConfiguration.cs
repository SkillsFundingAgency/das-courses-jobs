using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class CoursesApiClientConfiguration : IManagedIdentityClientConfiguration
    {
        public string ApiBaseUrl { get; set; }

        public string IdentifierUri { get; set; }

        public string Version { get; set; }
    }
}