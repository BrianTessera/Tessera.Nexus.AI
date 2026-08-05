using Dapper;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.Database;

public sealed class DatabaseHealthCheckService
    : IDatabaseHealthCheckService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseHealthCheckService(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> TestNexusAiConnectionAsync()
    {
        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var value = await connection.ExecuteScalarAsync<int>(
            "SELECT 1");

        return value == 1;
    }

    public async Task<bool> TestEpicorConnectionAsync()
    {
        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var value = await connection.ExecuteScalarAsync<int>(
            "SELECT 1");

        return value == 1;
    }
}