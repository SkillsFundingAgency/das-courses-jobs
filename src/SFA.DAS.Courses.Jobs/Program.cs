using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Extensions;

namespace SFA.DAS.Courses.Jobs
{
    static class Program
    {
        [ExcludeFromCodeCoverage]
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
                    services.ConfigureFromOptions(f => f.CoursesApiClientConfiguration);

                    var applicationConfig = context.Configuration
                        .Get<ApplicationConfiguration>()
                    ?? throw new InvalidOperationException("Configuration is missing or invalid.");

                    var coursesApiConfig = context.Configuration
                        .GetSection(nameof(CoursesApiClientConfiguration))
                        .Get<CoursesApiClientConfiguration>()
                    ?? throw new InvalidOperationException($"{nameof(CoursesApiClientConfiguration)} section is missing or invalid.");

                    services.AddServiceRegistrations(applicationConfig);
                    services.AddCoursesApi(coursesApiConfig);

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
