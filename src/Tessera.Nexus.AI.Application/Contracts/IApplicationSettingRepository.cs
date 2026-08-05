using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IApplicationSettingRepository
{
    Task<ApplicationSetting?> GetByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationSetting>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        ApplicationSetting setting,
        CancellationToken cancellationToken = default);
}