using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Provides access to Epicor metadata used to ground AI-generated SQL.
/// </summary>
public interface IEpicorMetadataRepository
{
    /// <summary>
    /// Searches metadata using a natural language term or keyword.
    /// </summary>
    Task<IReadOnlyList<EpicorDataTable>> SearchTablesAsync(
        string searchText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a specific table.
    /// </summary>
    Task<EpicorDataTable?> GetTableAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets fields for a specific Epicor table.
    /// </summary>
    Task<IReadOnlyList<EpicorDataField>> GetFieldsAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets relationships for a specific Epicor table.
    /// </summary>
    Task<IReadOnlyList<EpicorRelation>> GetRelationshipsAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets likely relevant tables for a user question.
    /// This is intended for AI prompt grounding.
    /// </summary>
    Task<IReadOnlyList<EpicorDataTable>> GetRelevantTablesAsync(
        string userQuestion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a full metadata bundle for a user question, including
    /// matching tables, fields, and relationships.
    /// </summary>
    Task<EpicorMetadataContext> GetMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a metadata bundle used to ground AI prompts.
/// </summary>
public sealed class EpicorMetadataContext
{
    public IReadOnlyList<EpicorDataTable> Tables { get; set; } =
        Array.Empty<EpicorDataTable>();

    public IReadOnlyList<EpicorDataField> Fields { get; set; } =
        Array.Empty<EpicorDataField>();

    public IReadOnlyList<EpicorRelation> Relationships { get; set; } =
        Array.Empty<EpicorRelation>();
}