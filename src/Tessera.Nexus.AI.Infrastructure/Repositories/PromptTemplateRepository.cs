using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class PromptTemplateRepository
    : IPromptTemplateRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PromptTemplateRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PromptTemplate?> GetByIdAsync(
        int promptTemplateId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                PromptTemplateId,
                TemplateName,
                TemplateType,
                TemplateText,
                VersionNumber,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.PromptTemplate
            WHERE PromptTemplateId = @PromptTemplateId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<PromptTemplate>(
            sql,
            new
            {
                PromptTemplateId = promptTemplateId
            });
    }

    public async Task<PromptTemplate?> GetActiveTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1
                PromptTemplateId,
                TemplateName,
                TemplateType,
                TemplateText,
                VersionNumber,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.PromptTemplate
            WHERE TemplateName = @TemplateName
              AND IsActive = 1
            ORDER BY VersionNumber DESC
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<PromptTemplate>(
            sql,
            new
            {
                TemplateName = templateName
            });
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                PromptTemplateId,
                TemplateName,
                TemplateType,
                TemplateText,
                VersionNumber,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.PromptTemplate
            ORDER BY
                TemplateName,
                VersionNumber DESC
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var results =
            await connection.QueryAsync<PromptTemplate>(sql);

        return results.ToList();
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                PromptTemplateId,
                TemplateName,
                TemplateType,
                TemplateText,
                VersionNumber,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM cfg.PromptTemplate
            WHERE TemplateType = @TemplateType
            ORDER BY
                TemplateName,
                VersionNumber DESC
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var results =
            await connection.QueryAsync<PromptTemplate>(
                sql,
                new
                {
                    TemplateType = templateType
                });

        return results.ToList();
    }

    public async Task<int> CreateAsync(
        PromptTemplate template,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO cfg.PromptTemplate
            (
                TemplateName,
                TemplateType,
                TemplateText,
                VersionNumber,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc
            )
            VALUES
            (
                @TemplateName,
                @TemplateType,
                @TemplateText,
                @VersionNumber,
                @IsActive,
                @EffectiveDate,
                @ExpirationDate,
                @ApprovedBy,
                @ApprovedDateUtc,
                @CreatedBy,
                SYSUTCDATETIME()
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        return await connection.ExecuteScalarAsync<int>(
            sql,
            template);
    }

    public async Task UpdateAsync(
        PromptTemplate template,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE cfg.PromptTemplate
            SET
                TemplateName = @TemplateName,
                TemplateType = @TemplateType,
                TemplateText = @TemplateText,
                IsActive = @IsActive,
                EffectiveDate = @EffectiveDate,
                ExpirationDate = @ExpirationDate,
                ApprovedBy = @ApprovedBy,
                ApprovedDateUtc = @ApprovedDateUtc,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE PromptTemplateId = @PromptTemplateId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            template);
    }

    public async Task DeactivateAsync(
        int promptTemplateId,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE cfg.PromptTemplate
            SET
                IsActive = 0,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE PromptTemplateId = @PromptTemplateId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                PromptTemplateId = promptTemplateId,
                ModifiedBy = modifiedBy
            });
    }
}