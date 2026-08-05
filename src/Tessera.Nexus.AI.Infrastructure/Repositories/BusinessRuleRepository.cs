using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class BusinessRuleRepository : IBusinessRuleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BusinessRuleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<BusinessRule?> GetByIdAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
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
            FROM ai.BusinessRule
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        return await connection.QuerySingleOrDefaultAsync<BusinessRule>(
            sql,
            new
            {
                BusinessRuleId = businessRuleId
            });
    }

    public async Task<IReadOnlyList<BusinessRule>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
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
            FROM ai.BusinessRule
            WHERE IsActive = 1
              AND (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CAST(SYSUTCDATETIME() AS date)
                  )
              AND (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CAST(SYSUTCDATETIME() AS date)
                  )
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(sql);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessRule>> GetByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
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
            FROM ai.BusinessRule
            WHERE IsActive = 1
              AND SemanticDomain = @SemanticDomain
              AND (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CAST(SYSUTCDATETIME() AS date)
                  )
              AND (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CAST(SYSUTCDATETIME() AS date)
                  )
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(
            sql,
            new
            {
                SemanticDomain = semanticDomain
            });

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessRule>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                BusinessRuleId,
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
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
            FROM ai.BusinessRule
            WHERE
                RuleName LIKE @Search
                OR RuleType LIKE @Search
                OR RuleCategory LIKE @Search
                OR RuleDescription LIKE @Search
                OR RuleLogicDescription LIKE @Search
                OR PromptInstruction LIKE @Search
                OR SemanticDomain LIKE @Search
                OR AppliesToTerm LIKE @Search
                OR AppliesToObjectType LIKE @Search
                OR AppliesToObjectName LIKE @Search
            ORDER BY
                Priority,
                RuleName;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        var results = await connection.QueryAsync<BusinessRule>(
            sql,
            new
            {
                Search = $"%{searchText}%"
            });

        return results.ToList();
    }

    public async Task<int> CreateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO ai.BusinessRule
            (
                RuleName,
                RuleType,
                RuleCategory,
                RuleDescription,
                RuleLogicDescription,
                SqlPredicate,
                PromptInstruction,
                SemanticDomain,
                Company,
                Plant,
                AppliesToTerm,
                AppliesToObjectType,
                AppliesToObjectName,
                OverridesBusinessRuleId,
                Priority,
                IsSystemRule,
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
            )
            VALUES
            (
                @RuleName,
                @RuleType,
                @RuleCategory,
                @RuleDescription,
                @RuleLogicDescription,
                @SqlPredicate,
                @PromptInstruction,
                @SemanticDomain,
                @Company,
                @Plant,
                @AppliesToTerm,
                @AppliesToObjectType,
                @AppliesToObjectName,
                @OverridesBusinessRuleId,
                @Priority,
                @IsSystemRule,
                @IsActive,
                @EffectiveDate,
                @ExpirationDate,
                @VersionNumber,
                @ApprovedBy,
                @ApprovedDateUtc,
                @CreatedBy,
                SYSUTCDATETIME(),
                @ModifiedBy,
                @ModifiedDateUtc
            );

            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        return await connection.ExecuteScalarAsync<int>(
            sql,
            rule);
    }

    public async Task UpdateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessRule
            SET
                RuleName = @RuleName,
                RuleType = @RuleType,
                RuleCategory = @RuleCategory,
                RuleDescription = @RuleDescription,
                RuleLogicDescription = @RuleLogicDescription,
                SqlPredicate = @SqlPredicate,
                PromptInstruction = @PromptInstruction,
                SemanticDomain = @SemanticDomain,
                Company = @Company,
                Plant = @Plant,
                AppliesToTerm = @AppliesToTerm,
                AppliesToObjectType = @AppliesToObjectType,
                AppliesToObjectName = @AppliesToObjectName,
                OverridesBusinessRuleId = @OverridesBusinessRuleId,
                Priority = @Priority,
                IsSystemRule = @IsSystemRule,
                IsActive = @IsActive,
                EffectiveDate = @EffectiveDate,
                ExpirationDate = @ExpirationDate,
                VersionNumber = @VersionNumber,
                ApprovedBy = @ApprovedBy,
                ApprovedDateUtc = @ApprovedDateUtc,
                ModifiedBy = @ModifiedBy,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            rule);
    }

    public async Task DeleteAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessRule
            SET
                IsActive = 0,
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessRuleId = @BusinessRuleId;
            """;

        using var connection = _connectionFactory.CreateNexusAiConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                BusinessRuleId = businessRuleId
            });
    }
}