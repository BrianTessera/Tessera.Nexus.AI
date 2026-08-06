using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class QueryHistoryRepository : IQueryHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public QueryHistoryRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> CreateAsync(
        QueryHistory queryHistory,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO ai.QueryHistory
            (
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            )
            VALUES
            (
                @UserQuestion,
                @TemplateName,
                @GeneratedPrompt,
                @GeneratedSql,
                @WasSuccessful,
                @SqlValidated,
                @QueryExecuted,
                @ResultResultRowCount,
                @GenerationElapsedMilliseconds,
                @ExecutionElapsedMilliseconds,
                @ErrorMessage,
                @ResultJson,
                @ModelName,
                @PromptTemplateName,
                @MetadataContextSummary,
                @RequestedBy,
                @SourcePage,
                SYSUTCDATETIME()
            );

            SELECT CAST(SCOPE_IDENTITY() AS bigint);
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                queryHistory,
                cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            command);
    }

    public async Task<QueryHistory?> GetByIdAsync(
        long queryHistoryId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            WHERE QueryHistoryId = @QueryHistoryId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    QueryHistoryId = queryHistoryId
                },
                cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<QueryHistory>(
            command);
    }

    public async Task<IReadOnlyList<QueryHistory>> GetRecentAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            ORDER BY CreatedDateUtc DESC,
                     QueryHistoryId DESC;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Take = NormalizeTake(take)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<QueryHistory>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<QueryHistory>> GetSuccessfulAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            WHERE WasSuccessful = 1
            ORDER BY CreatedDateUtc DESC,
                     QueryHistoryId DESC;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Take = NormalizeTake(take)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<QueryHistory>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<QueryHistory>> GetFailedAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            WHERE WasSuccessful = 0
            ORDER BY CreatedDateUtc DESC,
                     QueryHistoryId DESC;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Take = NormalizeTake(take)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<QueryHistory>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<QueryHistory>> GetByRequestedByAsync(
        string requestedBy,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestedBy))
        {
            return Array.Empty<QueryHistory>();
        }

        const string sql = """
            SELECT TOP (@Take)
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            WHERE RequestedBy = @RequestedBy
            ORDER BY CreatedDateUtc DESC,
                     QueryHistoryId DESC;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    RequestedBy = requestedBy.Trim(),
                    Take = NormalizeTake(take)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<QueryHistory>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<QueryHistory>> SearchByQuestionAsync(
        string searchText,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<QueryHistory>();
        }

        const string sql = """
            SELECT TOP (@Take)
                QueryHistoryId,
                UserQuestion,
                TemplateName,
                GeneratedPrompt,
                GeneratedSql,
                WasSuccessful,
                SqlValidated,
                QueryExecuted,
                ResultResultRowCount,
                GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds,
                ErrorMessage,
                ResultJson,
                ModelName,
                PromptTemplateName,
                MetadataContextSummary,
                RequestedBy,
                SourcePage,
                CreatedDateUtc
            FROM ai.QueryHistory
            WHERE UserQuestion LIKE @Search
            ORDER BY CreatedDateUtc DESC,
                     QueryHistoryId DESC;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Search = $"%{searchText.Trim()}%",
                    Take = NormalizeTake(take)
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<QueryHistory>(
                command);

        return results.ToList();
    }

    public async Task UpdateAsync(
        QueryHistory queryHistory,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ai.QueryHistory
            SET
                UserQuestion = @UserQuestion,
                TemplateName = @TemplateName,
                GeneratedPrompt = @GeneratedPrompt,
                GeneratedSql = @GeneratedSql,
                WasSuccessful = @WasSuccessful,
                SqlValidated = @SqlValidated,
                QueryExecuted = @QueryExecuted,
                ResultResultRowCount = @ResultResultRowCount,
                GenerationElapsedMilliseconds = @GenerationElapsedMilliseconds,
                ExecutionElapsedMilliseconds = @ExecutionElapsedMilliseconds,
                ErrorMessage = @ErrorMessage,
                ResultJson = @ResultJson,
                ModelName = @ModelName,
                PromptTemplateName = @PromptTemplateName,
                MetadataContextSummary = @MetadataContextSummary,
                RequestedBy = @RequestedBy,
                SourcePage = @SourcePage
            WHERE QueryHistoryId = @QueryHistoryId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                queryHistory,
                cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task DeleteAsync(
        long queryHistoryId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM ai.QueryHistory
            WHERE QueryHistoryId = @QueryHistoryId;
            """;

        using var connection =
            _connectionFactory.CreateNexusAiConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    QueryHistoryId = queryHistoryId
                },
                cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static int NormalizeTake(
        int take)
    {
        if (take <= 0)
        {
            return 100;
        }

        return take > 1000
            ? 1000
            : take;
    }
}