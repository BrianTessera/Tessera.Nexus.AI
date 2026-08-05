using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IMetadataRepository
{
    Task<IReadOnlyList<EpicorDataSet>> GetDataSetsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpicorDataTable>> GetTablesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpicorDataField>> GetFieldsAsync(
        string dataTableId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpicorRelation>> GetRelationsAsync(
        CancellationToken cancellationToken = default);

    Task SaveRefreshLogAsync(
        MetadataRefreshLog refreshLog,
        CancellationToken cancellationToken = default);
}