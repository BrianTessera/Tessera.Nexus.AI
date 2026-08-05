namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class MetadataRefreshLog
{
    public long MetadataRefreshLogId { get; set; }

    public string RefreshType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}