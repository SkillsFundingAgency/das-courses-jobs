using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Extensions;
using SFA.DAS.Courses.Jobs.Services;

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
                    try
                    {
                        services.AddApplicationInsightsTelemetryWorkerService();
                        services.ConfigureFunctionsApplicationInsights();
                        services.AddApplicationOptions();
                        services.ConfigureFromOptions(f => f.CoursesApiClientConfiguration);

                        var applicationConfig = context.Configuration
                            .Get<ApplicationConfiguration>()
                            ?? throw new InvalidOperationException("Configuration is missing or invalid.");

                        services.AddServiceRegistrations(applicationConfig);

                        var coursesApiConfig = context.Configuration
                            .GetSection(nameof(CoursesApiClientConfiguration))
                            .Get<CoursesApiClientConfiguration>()
                        ?? throw new InvalidOperationException($"{nameof(CoursesApiClientConfiguration)} section is missing or invalid.");

                        services.AddCoursesApi(coursesApiConfig);

                        services.AddHostedService<GitHubBearerTokenService>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception in ConfigureService: {ex}");
                        throw;
                    }
                })
                .ConfigureLogging(logging =>
                {
                    // This rule filters logs to capture only warnings and errors, removing this rule will allow Information logs to be captured
                    logging.Services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (defaultRule is not null)
                        {
                            options.Rules.Remove(defaultRule);
                        }
                    });
                })
                .Build();

                await host.RunAsync();
        }
    }
}
