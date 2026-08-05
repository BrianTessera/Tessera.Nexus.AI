using Microsoft.Extensions.DependencyInjection;

using Tessera.Nexus.AI.Application.Contracts;

using Tessera.Nexus.AI.Infrastructure.Database;
using Tessera.Nexus.AI.Infrastructure.Repositories;

namespace Tessera.Nexus.AI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // Database

        services.AddScoped<IDbConnectionFactory,
                           SqlConnectionFactory>();

        services.AddScoped<IDatabaseHealthCheckService,
                           DatabaseHealthCheckService>();

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