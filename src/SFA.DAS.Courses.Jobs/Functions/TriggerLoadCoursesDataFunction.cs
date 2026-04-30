using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Courses.Infrastructure.Api;

namespace SFA.DAS.Courses.Jobs.Functions;

public class TriggerLoadCoursesDataFunction(ICoursesApi _coursesApi)
{
    [Function(nameof(TriggerLoadCoursesDataFunction))]
    public async Task Run([TimerTrigger("%LoadCoursesDataSchedule%", RunOnStartup = false)] TimerInfo myTimer)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _coursesApi.LoadCoursesData(cts.Token);
    }
}
