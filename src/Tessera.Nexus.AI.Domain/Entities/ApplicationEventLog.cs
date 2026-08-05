namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class ApplicationEventLog
{
    public long ApplicationEventLogId { get; set; }

    public string EventLevel { get; set; } = string.Empty;

    public string EventSource { get; set; } = string.Empty;

    public string EventMessage { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }
}