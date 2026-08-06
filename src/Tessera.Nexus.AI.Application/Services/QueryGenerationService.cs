using System.Diagnostics;
using System.Text.Json;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Application.DTOs;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class QueryGenerationService : IQueryGenerationService
{
    private readonly IPromptBuilder _promptBuilder;
    private readonly ISqlGenerator _sqlGenerator;
    private readonly ISqlValidator _sqlValidator;
    private readonly IQueryExecutionService _queryExecutionService;

    public QueryGenerationService(
        IPromptBuilder promptBuilder,
        ISqlGenerator sqlGenerator,
        ISqlValidator sqlValidator,
        IQueryExecutionService queryExecutionService)
    {
        _promptBuilder = promptBuilder;
        _sqlGenerator = sqlGenerator;
        _sqlValidator = sqlValidator;
        _queryExecutionService = queryExecutionService;
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

            var generatedSql =
                await _sqlGenerator.GenerateSqlAsync(
                    prompt,
                    cancellationToken);

            var validationResult =
                _sqlValidator.Validate(generatedSql);

            response.GeneratedPrompt = prompt;
            response.GeneratedSql = generatedSql;
            response.SqlValidated = validationResult.IsValid;

            if (!validationResult.IsValid)
            {
                response.Success = false;

                response.ErrorMessage =
                    validationResult.ErrorMessage ??
                    string.Join(
                        Environment.NewLine,
                        validationResult.Violations);

                return response;
            }

            if (request.ExecuteQuery)
            {
                var executionResult =
                    await _queryExecutionService.ExecuteAsync(
                        generatedSql,
                        cancellationToken);

                response.RowCount = executionResult.RowCount;

                if (!executionResult.Success)
                {
                    response.Success = false;
                    response.ErrorMessage =
                        executionResult.ErrorMessage ??
                        "Query execution failed.";

                    response.ResultJson =
                        SerializeExecutionResult(
                            executionResult);

                    return response;
                }

                response.ResultJson =
                    SerializeExecutionResult(
                        executionResult);
            }

            response.Success = true;
            response.ErrorMessage = null;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;

            return response;
        }
        finally
        {
            stopwatch.Stop();

            response.ElapsedMilliseconds =
                stopwatch.ElapsedMilliseconds;
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