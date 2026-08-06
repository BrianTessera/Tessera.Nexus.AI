using System.Diagnostics;
using Dapper;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.Services;

public sealed class QueryExecutionService : IQueryExecutionService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISqlValidator _sqlValidator;

    public QueryExecutionService(
        IDbConnectionFactory connectionFactory,
        ISqlValidator sqlValidator)
    {
        _connectionFactory = connectionFactory;
        _sqlValidator = sqlValidator;
    }

    public async Task<QueryExecutionResult> ExecuteAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = new QueryExecutionResult
        {
            Sql = sql,
            Success = false
        };

        try
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                result.ErrorMessage = "SQL statement is required.";
                return result;
            }

            var validationResult = _sqlValidator.Validate(sql);

            if (!validationResult.IsValid)
            {
                result.ErrorMessage =
                    validationResult.ErrorMessage ??
                    string.Join(
                        Environment.NewLine,
                        validationResult.Violations);

                return result;
            }

            using var connection =
                _connectionFactory.CreateEpicorConnection();

            var command =
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken);

            var queryRows =
                await connection.QueryAsync(command);

            var rows =
                new List<IReadOnlyDictionary<string, object?>>();

            var columns =
                new List<string>();

            foreach (var queryRow in queryRows)
            {
                if (queryRow is not IDictionary<string, object?> dictionary)
                {
                    continue;
                }

                if (columns.Count == 0)
                {
                    columns.AddRange(dictionary.Keys);
                }

                var row =
                    dictionary.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase);

                rows.Add(row);
            }

            result.Columns = columns;
            result.Rows = rows;
            result.ResultRowCount = rows.Count;
            result.Success = true;

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;

            return result;
        }
        finally
        {
            stopwatch.Stop();

            result.ElapsedMilliseconds =
                stopwatch.ElapsedMilliseconds;
        }
    }
}