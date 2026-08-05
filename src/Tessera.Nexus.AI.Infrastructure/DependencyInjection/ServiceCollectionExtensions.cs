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
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

        return services;
    }
}