using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Jobs.Services;

namespace SFA.DAS.Courses.Jobs.Extensions
{
    public static class AddServiceRegistrationExtensions
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services)
        {
            var configuration = services
                .BuildServiceProvider()
                .GetRequiredService<ApplicationConfiguration>();

            services.AddHttpClient("github-contents", client =>
            {
                client.BaseAddress = new Uri(string.Format(ApplicationConfiguration.GitHubUrl, configuration.GitHubUserName, configuration.GitHubRepositoryName));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.GitHubAccessToken}");
                client.DefaultRequestHeaders.Add("User-Agent", "StandardsVersioning");
            });

            services.AddHttpClient("ifate", c => c.BaseAddress = new Uri(ApplicationConfiguration.ApprenticeshipStandardsUrl));

            services.AddTransient<IApprenticeshipStandardsService, ApprenticeshipStandardsService>();
            services.AddTransient<IGitHubRepositoryService, GitHubRepositoryService>();

            return services;
        }
    }
}

