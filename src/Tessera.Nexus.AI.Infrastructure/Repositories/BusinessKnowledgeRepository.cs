using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class BusinessKnowledgeRepository : IBusinessKnowledgeRepository
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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    BusinessKnowledgeId = businessKnowledgeId
                },
                cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<BusinessKnowledge>(
            command);
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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE
                IsActive = 1
                AND
                (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CONVERT(date, SYSUTCDATETIME())
                )
            ORDER BY
                Priority DESC,
                KnowledgeType,
                KnowledgeTitle;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> SearchAsync(
        string searchText,
        int maxResults = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        const string sql = """
            SELECT TOP (@MaxResults)
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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE
                IsActive = 1
                AND
                (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    KnowledgeTitle LIKE @Search
                    OR KnowledgeText LIKE @Search
                    OR SemanticDomain LIKE @Search
                    OR AppliesToTerm LIKE @Search
                    OR AppliesToObjectType LIKE @Search
                    OR AppliesToObjectName LIKE @Search
                    OR PromptInstruction LIKE @Search
                    OR KnowledgeType LIKE @Search
                )
            ORDER BY
                Priority DESC,
                KnowledgeType,
                KnowledgeTitle;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Search = $"%{searchText.Trim()}%",
                    MaxResults = NormalizeMaxResults(maxResults)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> GetRelevantKnowledgeAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        var searchTerms =
            ExtractSearchTerms(userQuestion);

        if (searchTerms.Count == 0)
        {
            searchTerms.Add(userQuestion.Trim());
        }

        const string sql = """
            SELECT TOP (@MaxResults)
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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE
                IsActive = 1
                AND
                (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    @SemanticDomain IS NULL
                    OR SemanticDomain IS NULL
                    OR SemanticDomain = @SemanticDomain
                )
                AND
                (
                    @Company IS NULL
                    OR Company IS NULL
                    OR Company = @Company
                )
                AND
                (
                    @Plant IS NULL
                    OR Plant IS NULL
                    OR Plant = @Plant
                )
                AND
                (
                    KnowledgeTitle LIKE @QuestionSearch
                    OR KnowledgeText LIKE @QuestionSearch
                    OR AppliesToTerm LIKE @QuestionSearch
                    OR AppliesToObjectName LIKE @QuestionSearch
                    OR PromptInstruction LIKE @QuestionSearch

                    OR KnowledgeTitle LIKE @Term1
                    OR KnowledgeText LIKE @Term1
                    OR AppliesToTerm LIKE @Term1
                    OR AppliesToObjectName LIKE @Term1
                    OR PromptInstruction LIKE @Term1

                    OR KnowledgeTitle LIKE @Term2
                    OR KnowledgeText LIKE @Term2
                    OR AppliesToTerm LIKE @Term2
                    OR AppliesToObjectName LIKE @Term2
                    OR PromptInstruction LIKE @Term2

                    OR KnowledgeTitle LIKE @Term3
                    OR KnowledgeText LIKE @Term3
                    OR AppliesToTerm LIKE @Term3
                    OR AppliesToObjectName LIKE @Term3
                    OR PromptInstruction LIKE @Term3

                    OR KnowledgeTitle LIKE @Term4
                    OR KnowledgeText LIKE @Term4
                    OR AppliesToTerm LIKE @Term4
                    OR AppliesToObjectName LIKE @Term4
                    OR PromptInstruction LIKE @Term4

                    OR KnowledgeTitle LIKE @Term5
                    OR KnowledgeText LIKE @Term5
                    OR AppliesToTerm LIKE @Term5
                    OR AppliesToObjectName LIKE @Term5
                    OR PromptInstruction LIKE @Term5
                )
            ORDER BY
                Priority DESC,
                CASE
                    WHEN AppliesToTerm IS NOT NULL THEN 0
                    WHEN PromptInstruction IS NOT NULL THEN 1
                    ELSE 2
                END,
                KnowledgeType,
                KnowledgeTitle;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    QuestionSearch = $"%{userQuestion.Trim()}%",
                    Term1 = ToLikeTerm(searchTerms.ElementAtOrDefault(0)),
                    Term2 = ToLikeTerm(searchTerms.ElementAtOrDefault(1)),
                    Term3 = ToLikeTerm(searchTerms.ElementAtOrDefault(2)),
                    Term4 = ToLikeTerm(searchTerms.ElementAtOrDefault(3)),
                    Term5 = ToLikeTerm(searchTerms.ElementAtOrDefault(4)),
                    SemanticDomain = NormalizeNullable(semanticDomain),
                    Company = NormalizeNullable(company),
                    Plant = NormalizeNullable(plant),
                    MaxResults = NormalizeMaxResults(maxResults)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> GetBySemanticDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(semanticDomain))
        {
            return Array.Empty<BusinessKnowledge>();
        }

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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE
                IsActive = 1
                AND SemanticDomain = @SemanticDomain
                AND
                (
                    EffectiveDate IS NULL
                    OR EffectiveDate <= CONVERT(date, SYSUTCDATETIME())
                )
                AND
                (
                    ExpirationDate IS NULL
                    OR ExpirationDate >= CONVERT(date, SYSUTCDATETIME())
                )
            ORDER BY
                Priority DESC,
                KnowledgeType,
                KnowledgeTitle;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    SemanticDomain = semanticDomain.Trim()
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> GetExpiringKnowledgeAsync(
        DateOnly cutoffDate,
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
                ModifiedDateUtc,
                RowVersion
            FROM ai.BusinessKnowledge
            WHERE
                IsActive = 1
                AND ExpirationDate IS NOT NULL
                AND ExpirationDate <= @CutoffDate
            ORDER BY
                ExpirationDate,
                Priority DESC,
                KnowledgeTitle;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    CutoffDate =
                        cutoffDate.ToDateTime(
                            TimeOnly.MinValue)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<BusinessKnowledge>(
                command);

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
                COALESCE(NULLIF(@CreatedBy, ''), SUSER_SNAME()),
                SYSUTCDATETIME()
            );

            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                knowledge,
                cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(
            command);
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
                ModifiedBy = COALESCE(NULLIF(@ModifiedBy, ''), SUSER_SNAME()),
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                knowledge,
                cancellationToken: cancellationToken);

        await connection.ExecuteAsync(
            command);
    }

    public async Task RetireAsync(
        int businessKnowledgeId,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.BusinessKnowledge
            SET
                IsActive = 0,
                ModifiedBy = COALESCE(NULLIF(@ModifiedBy, ''), SUSER_SNAME()),
                ModifiedDateUtc = SYSUTCDATETIME()
            WHERE BusinessKnowledgeId = @BusinessKnowledgeId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    BusinessKnowledgeId = businessKnowledgeId,
                    ModifiedBy = modifiedBy
                },
                cancellationToken: cancellationToken);

        await connection.ExecuteAsync(
            command);
    }

    public async Task<bool> ExistsAsync(
        string knowledgeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(knowledgeTitle))
        {
            return false;
        }

        const string sql = """
            SELECT
                CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM ai.BusinessKnowledge
                        WHERE KnowledgeTitle = @KnowledgeTitle
                    )
                    THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    KnowledgeTitle = knowledgeTitle.Trim()
                },
                cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            command);
    }

    private static int NormalizeMaxResults(
        int maxResults)
    {
        if (maxResults <= 0)
        {
            return 10;
        }

        return maxResults > 100
            ? 100
            : maxResults;
    }

    private static string? NormalizeNullable(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string ToLikeTerm(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "%___no_match___%";
        }

        return $"%{value.Trim()}%";
    }

    private static List<string> ExtractSearchTerms(
        string userQuestion)
    {
        var ignoredWords =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "a",
                "an",
                "and",
                "are",
                "by",
                "for",
                "from",
                "get",
                "give",
                "i",
                "in",
                "is",
                "last",
                "latest",
                "list",
                "me",
                "of",
                "on",
                "or",
                "please",
                "recent",
                "select",
                "show",
                "the",
                "to",
                "top",
                "want",
                "what",
                "which",
                "with"
            };

        return userQuestion
            .Split(
                new[]
                {
                    ' ',
                    '\t',
                    '\r',
                    '\n',
                    ',',
                    '.',
                    ';',
                    ':',
                    '/',
                    '\\',
                    '(',
                    ')',
                    '[',
                    ']',
                    '{',
                    '}',
                    '"'
                },
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(term => term.Length >= 3)
            .Where(term => !ignoredWords.Contains(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }
}