namespace Tessera.Nexus.AI.Domain.Entities;

public sealed class ApplicationSetting
{
    public int ApplicationSettingId { get; set; }

    public string SettingKey { get; set; } = string.Empty;

    public string? SettingValue { get; set; }

    public string? Description { get; set; }

    public bool IsSensitive { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDateUtc { get; set; }
}