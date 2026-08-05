namespace Tessera.Nexus.AI.Application.Contracts;

public interface IMetadataRefreshService
{
    Task RefreshAsync(
        CancellationToken cancellationToken = default);
}