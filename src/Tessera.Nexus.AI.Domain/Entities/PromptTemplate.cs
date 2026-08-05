namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class PromptTemplate
{
    public int PromptTemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public string TemplateType { get; set; } = string.Empty;

    public string TemplateText { get; set; } = string.Empty;

    public int VersionNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedDateUtc { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDateUtc { get; set; }
}