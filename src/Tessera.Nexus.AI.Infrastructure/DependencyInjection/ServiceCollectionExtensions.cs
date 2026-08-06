using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Application.Services;

using Tessera.Nexus.AI.Infrastructure.AI;
using Tessera.Nexus.AI.Infrastructure.Configuration;
using Tessera.Nexus.AI.Infrastructure.Database;
using Tessera.Nexus.AI.Infrastructure.Repositories;
namespace Tessera.Nexus.AI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // Database

        services.AddScoped<ISqlValidator, SqlValidator>();
        services.AddScoped<IDbConnectionFactory,
                           SqlConnectionFactory>();

        services.AddScoped<IDatabaseHealthCheckService,
                           DatabaseHealthCheckService>();
        // Application Services
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IQueryGenerationService, QueryGenerationService>();
        services.AddScoped<ISqlValidator, SqlValidator>();

        // AI Services
        //services.AddScoped<ISqlGenerator, MockSqlGenerator>();

        services.AddScoped<ISqlGenerator, OllamaSqlGenerator>();

        services.AddHttpClient<IOllamaClient, OllamaClient>(
            (serviceProvider, httpClient) =>
            {
                var settings =
                    serviceProvider
                        .GetRequiredService<IOptions<OllamaSettings>>()
                        .Value;

                httpClient.BaseAddress =
                    new Uri(settings.BaseUrl.TrimEnd('/') + "/");

                httpClient.Timeout =
                    TimeSpan.FromSeconds(settings.TimeoutSeconds);
            });

        // Repositories

        services.AddScoped<IApplicationSettingRepository,
                           ApplicationSettingRepository>();

        services.AddScoped<IPromptTemplateRepository,
                           PromptTemplateRepository>();

        services.AddScoped <IBusinessKnowledgeRepository,
            BusinessKnowledgeRepository > ();

        services.AddScoped<IBusinessRuleRepository, BusinessRuleRepository>();

        return services;
    }
}