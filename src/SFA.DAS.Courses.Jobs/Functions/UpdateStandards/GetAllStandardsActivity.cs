using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.Functions.UpdateStandards
{
    public class GetAllStandardsActivity
    {
        private readonly ILogger<GetAllStandardsActivity> _logger;
        private readonly IApprenticeshipStandardsService _apprenticeshipStandardsService;

        public GetAllStandardsActivity(
            ILogger<GetAllStandardsActivity> logger,
            IApprenticeshipStandardsService apprenticeshipStandardsService)
        {
            _logger = logger;
            _apprenticeshipStandardsService = apprenticeshipStandardsService;
        }

        [Function(nameof(GetAllStandardsActivity))]
        public async Task<Dictionary<string, string>> Run([ActivityTrigger] object input)
        {
            var standards = await _apprenticeshipStandardsService.GetAllStandards();
            _logger.LogInformation("Retrieved {Count} standards", standards.Count);
            return standards;
        }
    }
}
