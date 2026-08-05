namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class BusinessRule
{
    public int BusinessRuleId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public string RuleType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}