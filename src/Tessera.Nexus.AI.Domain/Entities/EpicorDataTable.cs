namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class EpicorDataTable
{
    public long EpicorDataTableId { get; set; }

    public string DataTableId { get; set; } = string.Empty;

    public string? SchemaName { get; set; }

    public string? DbTableName { get; set; }
}