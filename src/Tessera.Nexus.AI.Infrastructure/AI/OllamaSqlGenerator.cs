using System.Text.RegularExpressions;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.AI;

/// <summary>
/// Generates SQL by sending the completed prompt to Ollama.
/// </summary>
public sealed class OllamaSqlGenerator : ISqlGenerator
{
    private readonly IOllamaClient _ollamaClient;

    public OllamaSqlGenerator(IOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }

    public async Task<string> GenerateSqlAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));
        }

        var response =
            await _ollamaClient.GenerateAsync(
                prompt,
                cancellationToken);

        var sql = ExtractSql(response);

        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new InvalidOperationException(
                "Ollama did not return a usable SQL statement.");
        }

        return sql;
    }

    private static string ExtractSql(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return string.Empty;
        }

        var cleaned = response.Trim();

        cleaned = RemoveMarkdownCodeFence(cleaned);

        cleaned = RemoveCommonPrefixes(cleaned);

        cleaned = cleaned.Trim();

        return cleaned;
    }

    private static string RemoveMarkdownCodeFence(string text)
    {
        var fencedSqlMatch = Regex.Match(
            text,
            @"```(?:sql|tsql)?\s*(.*?)```",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (fencedSqlMatch.Success)
        {
            return fencedSqlMatch.Groups[1].Value.Trim();
        }

        return text
            .Replace("```sql", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```tsql", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string RemoveCommonPrefixes(string text)
    {
        var cleaned = text.Trim();

        var prefixes = new[]
        {
            "SQL:",
            "T-SQL:",
            "Generated SQL:",
            "Here is the SQL:",
            "Here is the T-SQL:",
            "The SQL is:"
        };

        foreach (var prefix in prefixes)
        {
            if (cleaned.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[prefix.Length..].Trim();
                break;
            }
        }

        return cleaned;
    }
}