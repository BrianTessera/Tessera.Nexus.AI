using Dapper;

using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class BusinessKnowledgeRepository
    : IBusinessKnowledgeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BusinessKnowledgeRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<BusinessKnowledge?> GetByIdAsync(
        int businessKnowledgeId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessKnowledgeId,
                KnowledgeType,
                KnowledgeTitle,
                KnowledgeText,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                PromptInstruction,
                Priority,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessKnowledge
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<BusinessKnowledge>(
            sql,
            new
            {
                BusinessKnowledgeId = businessKnowledgeId
            });
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessKnowledgeId,
                KnowledgeType,
                KnowledgeTitle,
                KnowledgeText,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                PromptInstruction,
                Priority,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessKnowledge
            WHERE IsActive = 1
            ORDER BY
                Priority,
                KnowledgeTitle
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var results =
            await connection.QueryAsync<BusinessKnowledge>(sql);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessKnowledgeId,
                KnowledgeType,
                KnowledgeTitle,
                KnowledgeText,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                PromptInstruction,
                Priority,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc,
                ModifiedBy,
                ModifiedDateUtc
            FROM ai.BusinessKnowledge
            WHERE
                KnowledgeTitle LIKE @Search
                OR KnowledgeText LIKE @Search
                OR AppliesToTerm LIKE @Search
                OR SemanticDomain LIKE @Search
            ORDER BY
                Priority,
                KnowledgeTitle
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                sql,
                new
                {
                    Search = $"%{searchText}%"
                });

        return results.ToList();
    }

    public async Task<int> CreateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO ai.BusinessKnowledge
            (
                KnowledgeType,
                KnowledgeTitle,
                KnowledgeText,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                PromptInstruction,
                Priority,
                IsActive,
                EffectiveDate,
                ExpirationDate,
                VersionNumber,
                ApprovedBy,
                ApprovedDateUtc,
                CreatedBy,
                CreatedDateUtc
            )
            VALUES
            (
                @KnowledgeType,
                @KnowledgeTitle,
                @KnowledgeText,
                @SemanticDomain,
                @Company,
                @Plant,
                @AppliesToTerm,
                @AppliesToObjectType,
                @AppliesToObjectName,
                @PromptInstruction,
                @Priority,
                @IsActive,
                @EffectiveDate,
                @ExpirationDate,
                @VersionNumber,
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
            knowledge);
    }

    public async Task UpdateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessKnowledge
            SET
                KnowledgeType = @KnowledgeType,
                KnowledgeTitle = @KnowledgeTitle,
                KnowledgeText = @KnowledgeText,
                SemanticDomain = @SemanticDomain,
                Company = @Company,
                Plant = @Plant,
                AppliesToTerm = @AppliesToTerm,
                AppliesToObjectType = @AppliesToObjectType,
                AppliesToObjectName = @AppliesToObjectName,
                PromptInstruction = @PromptInstruction,
                Priority = @Priority,
                IsActive = @IsActive,
                EffectiveDate = @EffectiveDate,
                ExpirationDate = @ExpirationDate,
                VersionNumber = @VersionNumber,
                ApprovedBy = @ApprovedBy,
                ApprovedDateUtc = @ApprovedDateUtc,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            knowledge);
    }

    public async Task DeleteAsync(
        int businessKnowledgeId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessKnowledge
            SET
                IsActive = 0,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                BusinessKnowledgeId = businessKnowledgeId
            });
    }
}