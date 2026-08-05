using System.Data;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IDbConnectionFactory
{
    IDbConnection CreateNexusAiConnection();

    IDbConnection CreateEpicorConnection();
}