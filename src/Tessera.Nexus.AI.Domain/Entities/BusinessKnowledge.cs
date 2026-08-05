namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class BusinessKnowledge
{
    public int BusinessKnowledgeId { get; set; }

    public string KnowledgeType { get; set; } = string.Empty;

    public string KnowledgeTitle { get; set; } = string.Empty;

    public string KnowledgeText { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}