namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class BusinessRule
{
    public int BusinessRuleId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public string RuleType { get; set; } = string.Empty;

    public string? RuleCategory { get; set; }

    public string? RuleDescription { get; set; }

    public string? RuleLogicDescription { get; set; }

    public string? SqlPredicate { get; set; }

    public string? PromptInstruction { get; set; }

    public string? SemanticDomain { get; set; }

    public string? Company { get; set; }

    public string? Plant { get; set; }

    public string? AppliesToTerm { get; set; }

    public string? AppliesToObjectType { get; set; }

    public string? AppliesToObjectName { get; set; }

    public int? OverridesBusinessRuleId { get; set; }

    public int Priority { get; set; }

    public bool IsSystemRule { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public int VersionNumber { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDateUtc { get; set; }
}