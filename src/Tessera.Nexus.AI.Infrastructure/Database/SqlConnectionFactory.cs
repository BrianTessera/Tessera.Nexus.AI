using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.Database;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _nexusAiConnectionString;
    private readonly string _epicorConnectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _nexusAiConnectionString =
            configuration.GetConnectionString("NexusAI")
            ?? throw new InvalidOperationException(
                "Connection string 'NexusAI' was not found.");

        _epicorConnectionString =
            configuration.GetConnectionString("Epicor")
            ?? throw new InvalidOperationException(
                "Connection string 'Epicor' was not found.");
    }

    public IDbConnection CreateNexusAiConnection()
    {
        return new SqlConnection(_nexusAiConnectionString);
    }

    public IDbConnection CreateEpicorConnection()
    {
        return new SqlConnection(_epicorConnectionString);
    }
}
