using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class ApplicationSettingRepository
    : IApplicationSettingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ApplicationSettingRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ApplicationSetting?> GetByKeyAsync(
        string settingKey,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                ApplicationSettingId,
                SettingKey,
                SettingValue,
                Description,
                IsSensitive,
                IsActive,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.ApplicationSetting
            WHERE SettingKey = @SettingKey
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<ApplicationSetting>(
            sql,
            new
            {
                SettingKey = settingKey
            });
    }

    public async Task<IReadOnlyList<ApplicationSetting>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                ApplicationSettingId,
                SettingKey,
                SettingValue,
                Description,
                IsSensitive,
                IsActive,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.ApplicationSetting
            ORDER BY SettingKey
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var results =
            await connection.QueryAsync<ApplicationSetting>(sql);

        return results.ToList();
    }

    public async Task UpdateAsync(
        ApplicationSetting setting,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE cfg.ApplicationSetting
            SET
                SettingValue = @SettingValue,
                Description = @Description,
                IsSensitive = @IsSensitive,
                IsActive = @IsActive,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE ApplicationSettingId = @ApplicationSettingId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                setting.ApplicationSettingId,
                setting.SettingValue,
                setting.Description,
                setting.IsSensitive,
                setting.IsActive,
                setting.ModifiedBy
            });
    }
}