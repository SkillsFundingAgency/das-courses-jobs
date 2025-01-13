using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Courses.Jobs.Extensions;

namespace SFA.DAS.Courses.Jobs
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddApplicationOptions();
                    services.AddServiceRegistrations();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddLogging();
                })
                .Build();

                await host.RunAsync();
        }
    }
}
