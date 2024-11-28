using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Courses.Jobs.Functions.Example
{
    public class ExampleActivity
    {
        private readonly ILogger<ExampleActivity> _logger;

        public ExampleActivity(
            ILogger<ExampleActivity> logger)
        {
            _logger = logger;
        }

        [Function("ExampleActivity")]
        public void RunActivity([ActivityTrigger] string name)
        {
            _logger.LogInformation("{ActivityName} started at {DateTimeNow}", nameof(ExampleActivity), DateTime.Now);
        }
    }
}
