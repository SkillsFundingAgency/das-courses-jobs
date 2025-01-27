using RestEase;

namespace SFA.DAS.Courses.Infrastructure.Api
{
    public interface ICoursesApi
    {
        [Get("/ops/dataload/StandardsImportUrl")]
        Task<string> GetStandardsImportUrl();
    }
}