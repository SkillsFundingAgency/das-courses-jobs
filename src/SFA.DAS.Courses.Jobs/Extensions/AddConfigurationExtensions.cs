using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Courses.Infrastructure.Configuration;

namespace SFA.DAS.Courses.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddConfigurationExtensions
{
    public static void AddConfiguration(this IConfigurationBuilder builder)
    {
        var config = builder.Build();
        builder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = config["ConfigNames"]?.Split(",") ?? [];
            options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        });
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services)
    {
        services
            .AddOptions<ApplicationConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
                configuration.Bind(settings));

        return services;
    }
}
