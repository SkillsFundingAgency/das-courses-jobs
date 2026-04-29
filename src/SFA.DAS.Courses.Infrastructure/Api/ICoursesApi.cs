using Refit;

namespace SFA.DAS.Courses.Infrastructure.Api;

public interface ICoursesApi
{
    [Post("/ops/dataload")]
    Task LoadCoursesData(CancellationToken cancellationToken);
}