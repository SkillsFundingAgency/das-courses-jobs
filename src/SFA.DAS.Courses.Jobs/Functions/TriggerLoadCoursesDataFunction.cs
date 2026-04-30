using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using SFA.DAS.Courses.Infrastructure.Api;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Functions;

public class TriggerLoadCoursesDataFunction(ICoursesApi _coursesApi, IOptions<ApplicationConfiguration> _configuration)
{
    [Function(nameof(TriggerLoadCoursesDataFunction))]
    public async Task Run([TimerTrigger("%LoadCoursesDataSchedule%", RunOnStartup = false)] TimerInfo myTimer)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(_configuration.Value.FunctionsConfiguration.LoadCoursesTimeoutInMinutes));
        await _coursesApi.LoadCoursesData(cts.Token);
    }
}
