namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class EpicorDataField
{
    public long EpicorDataFieldId { get; set; }

    public string DataTableId { get; set; } = string.Empty;

    public string FieldName { get; set; } = string.Empty;
}
