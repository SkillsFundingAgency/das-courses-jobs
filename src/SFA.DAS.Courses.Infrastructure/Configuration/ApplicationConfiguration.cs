using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Courses.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApplicationConfiguration
    {
        public CoursesApiClientConfiguration CoursesApiClientConfiguration { get; set; }
        public InstituteOfApprenticeshipsApiConfiguration InstituteOfApprenticeshipsApiConfiguration { get; set; }
        public FunctionsConfiguration FunctionsConfiguration { get; set; }
    }
}
