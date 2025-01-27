
namespace SFA.DAS.Courses.Jobs.Services
{
    public interface IApprenticeshipStandardsService
    {
        Task<Dictionary<string, string>> GetAllStandards();
    }
}