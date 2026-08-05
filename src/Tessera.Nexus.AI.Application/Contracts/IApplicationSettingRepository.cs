namespace Tessera.Nexus.AI.Application.Contracts;

public interface IApplicationSettingRepository
{
    Task<string?> GetValueAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, string?>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        string settingKey,
        string? value,
        string modifiedBy,
        CancellationToken cancellationToken = default);
}