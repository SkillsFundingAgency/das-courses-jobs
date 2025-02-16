﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.Courses.Infrastructure.Api;
using SFA.DAS.Courses.Infrastructure.Configuration;
using SFA.DAS.Courses.Infrastructure.HttpRequestHandlers;
using SFA.DAS.Courses.Jobs.Services;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.MessageHandlers;
using SFA.DAS.Http.TokenGenerators;

namespace SFA.DAS.Courses.Jobs.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class AddServiceRegistrationExtensions
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, ApplicationConfiguration configuration)
        {
            services.AddHttpClient("github-contents", client =>
            {
                client.BaseAddress = new Uri(string.Format(ApplicationConfiguration.GitHubUrl, configuration.GitHubUserName, configuration.GitHubRepositoryName));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.GitHubAccessToken}");
                client.DefaultRequestHeaders.Add("User-Agent", "StandardsVersioning");
            });
            
            services.AddHttpClient("ifate");

            services.AddTransient<IApprenticeshipStandardsService, ApprenticeshipStandardsService>();
            services.AddTransient<IGitHubRepositoryService, GitHubRepositoryService>();

            return services;
        }

        public static IServiceCollection AddCoursesApi(this IServiceCollection services, CoursesApiClientConfiguration configuration)
        {
            services.AddScoped<DefaultHeadersHandler>();
            services.AddScoped<LoggingMessageHandler>();
            services.AddScoped<ManagedIdentityHeadersHandler>();
            services.AddScoped(sp => new VersionHeaderHandler(configuration.Version));

            services
                .AddRestEaseClient<ICoursesApi>(configuration.ApiBaseUrl)
                .AddHttpMessageHandler<DefaultHeadersHandler>()
                .AddHttpMessageHandler<LoggingMessageHandler>()
                .AddHttpMessageHandler<ManagedIdentityHeadersHandler>()
                .AddHttpMessageHandler<VersionHeaderHandler>();

            services.AddTransient<IManagedIdentityClientConfiguration>((_) => configuration);
            services.AddTransient<IManagedIdentityTokenGenerator, ManagedIdentityTokenGenerator>();

            return services;
        }
    }
}

