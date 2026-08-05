using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.AI;

/// <summary>
/// Temporary SQL generator used during development
/// before Ollama integration is implemented.
/// </summary>
public sealed class MockSqlGenerator : ISqlGenerator
{
    public Task<string> GenerateSqlAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT TOP (100)
                PartNum,
                PartDescription,
                TypeCode
            FROM Erp.Part
            ORDER BY PartNum;
            """;

        return Task.FromResult(sql);
    }
}