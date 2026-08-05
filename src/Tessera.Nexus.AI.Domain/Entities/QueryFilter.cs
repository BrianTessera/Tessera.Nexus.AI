namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class QueryFilter
{
    public int QueryFilterId { get; set; }

    public string FilterName { get; set; } = string.Empty;

    public string SqlPredicate { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}