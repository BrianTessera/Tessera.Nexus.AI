using System.Diagnostics;
using System.Text.Json;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Application.DTOs;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class QueryGenerationService : IQueryGenerationService
{
    private readonly IPromptBuilder _promptBuilder;
    private readonly ISqlGenerator _sqlGenerator;
    private readonly ISqlValidator _sqlValidator;
    private readonly IQueryExecutionService _queryExecutionService;
    private readonly IQueryHistoryRepository _queryHistoryRepository;

    public QueryGenerationService(
        IPromptBuilder promptBuilder,
        ISqlGenerator sqlGenerator,
        ISqlValidator sqlValidator,
        IQueryExecutionService queryExecutionService,
        IQueryHistoryRepository queryHistoryRepository)
    {
        _promptBuilder = promptBuilder;
        _sqlGenerator = sqlGenerator;
        _sqlValidator = sqlValidator;
        _queryExecutionService = queryExecutionService;
        _queryHistoryRepository = queryHistoryRepository;
    }

    public async Task<QueryResponse> GenerateQueryAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = new QueryResponse
        {
            ProcessedUtc = DateTime.UtcNow,
            Success = false,
            SqlValidated = false
        };

        var history = new QueryHistory
        {
            UserQuestion = request?.UserQuestion ?? string.Empty,
            TemplateName = request?.TemplateName,
            QueryExecuted = request?.ExecuteQuery ?? false,
            SourcePage = "QueryGeneratorPage",
            CreatedDateUtc = DateTime.UtcNow
        };

        try
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            response.UserQuestion = request.UserQuestion;

            if (string.IsNullOrWhiteSpace(request.UserQuestion))
            {
                throw new ArgumentException(
                    "User question is required.",
                    nameof(request));
            }

            var prompt =
                string.IsNullOrWhiteSpace(request.TemplateName)
                    ? await _promptBuilder.BuildSqlPromptAsync(
                        request.UserQuestion,
                        cancellationToken)
                    : await _promptBuilder.BuildPromptAsync(
                        request.TemplateName,
                        request.UserQuestion,
                        cancellationToken);

            response.GeneratedPrompt = prompt;
            history.GeneratedPrompt = prompt;

            var generatedSql =
                await _sqlGenerator.GenerateSqlAsync(
                    prompt,
                    cancellationToken);

            response.GeneratedSql = generatedSql;
            history.GeneratedSql = generatedSql;

            var validationResult =
                _sqlValidator.Validate(generatedSql);

            response.SqlValidated = validationResult.IsValid;
            history.SqlValidated = validationResult.IsValid;

            if (!validationResult.IsValid)
            {
                response.Success = false;

                response.ErrorMessage =
                    validationResult.ErrorMessage ??
                    string.Join(
                        Environment.NewLine,
                        validationResult.Violations);

                history.WasSuccessful = false;
                history.ErrorMessage = response.ErrorMessage;

                return response;
            }

            if (request.ExecuteQuery)
            {
                var executionResult =
                    await _queryExecutionService.ExecuteAsync(
                        generatedSql,
                        cancellationToken);

                response.RowCount = executionResult.RowCount;

                history.ResultRowCount = executionResult.RowCount;
                history.ExecutionElapsedMilliseconds =
                    executionResult.ElapsedMilliseconds;

                response.ResultJson =
                    SerializeExecutionResult(
                        executionResult);

                history.ResultJson = response.ResultJson;

                if (!executionResult.Success)
                {
                    response.Success = false;

                    response.ErrorMessage =
                        executionResult.ErrorMessage ??
                        "Query execution failed.";

                    history.WasSuccessful = false;
                    history.ErrorMessage = response.ErrorMessage;

                    return response;
                }
            }

            response.Success = true;
            response.ErrorMessage = null;

            history.WasSuccessful = true;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;

            history.WasSuccessful = false;
            history.ErrorMessage = ex.Message;

            return response;
        }
        finally
        {
            stopwatch.Stop();

            response.ElapsedMilliseconds =
                stopwatch.ElapsedMilliseconds;

            history.GenerationElapsedMilliseconds =
                stopwatch.ElapsedMilliseconds;

            try
            {
                await _queryHistoryRepository.CreateAsync(
                    history,
                    cancellationToken);
            }
            catch
            {
                // Query history logging must never break query generation.
            }
        }
    }

    private static string SerializeExecutionResult(
        QueryExecutionResult executionResult)
    {
        var payload = new
        {
            executionResult.Success,
            executionResult.Sql,
            executionResult.ErrorMessage,
            executionResult.RowCount,
            executionResult.ElapsedMilliseconds,
            executionResult.Columns,
            executionResult.Rows
        };

        return JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}