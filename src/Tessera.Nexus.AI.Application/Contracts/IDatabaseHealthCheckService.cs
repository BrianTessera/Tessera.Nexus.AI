namespace Tessera.Nexus.AI.Application.Contracts;

public interface IDatabaseHealthCheckService
{
    Task<bool> TestNexusAiConnectionAsync();

    Task<bool> TestEpicorConnectionAsync();
}