using System.Text;
using System.Text.RegularExpressions;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Infrastructure.AI;

/// <summary>
/// Generates SQL by sending the completed prompt to Ollama.
/// Includes defensive retry handling for empty or malformed model responses.
/// </summary>
public sealed class OllamaSqlGenerator : ISqlGenerator
{
    private readonly IOllamaClient _ollamaClient;

    public OllamaSqlGenerator(
        IOllamaClient ollamaClient)
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

        var primaryPrompt =
            BuildPrimaryPrompt(prompt);

        var response =
            await GenerateWithRetryAsync(
                primaryPrompt,
                prompt,
                cancellationToken);

        var sql =
            ExtractSql(response);

        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new InvalidOperationException(
                $"Ollama did not return a usable SQL statement. PromptLength={prompt.Length}");
        }

        return sql;
    }

    private async Task<string> GenerateWithRetryAsync(
        string primaryPrompt,
        string originalPrompt,
        CancellationToken cancellationToken)
    {
        try
        {
            var response =
                await _ollamaClient.GenerateAsync(
                    primaryPrompt,
                    cancellationToken);

            if (!string.IsNullOrWhiteSpace(response))
            {
                return response;
            }
        }
        catch (InvalidOperationException ex)
            when (IsEmptyResponseException(ex))
        {
            // Retry below with a shorter, stricter prompt.
        }

        var retryPrompt =
            BuildRetryPrompt(originalPrompt);

        try
        {
            var retryResponse =
                await _ollamaClient.GenerateAsync(
                    retryPrompt,
                    cancellationToken);

            if (!string.IsNullOrWhiteSpace(retryResponse))
            {
                return retryResponse;
            }
        }
        catch (InvalidOperationException ex)
            when (IsEmptyResponseException(ex))
        {
            throw new InvalidOperationException(
                $"""
        Ollama returned an empty generated response after retry.

        PromptLength: {originalPrompt.Length}

        Inner Error:
        {ex.Message}
        """,
                ex);
        }

        throw new InvalidOperationException(
            $"Ollama returned an empty generated response. PromptLength={originalPrompt.Length}");
    }

    private static bool IsEmptyResponseException(
        InvalidOperationException exception)
    {
        return exception.Message.Contains(
            "empty generated response",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildPrimaryPrompt(
        string prompt)
    {
        var builder = new StringBuilder();

        builder.AppendLine(prompt.Trim());
        builder.AppendLine();
        builder.AppendLine("### FINAL OUTPUT REQUIREMENTS ###");
        builder.AppendLine("- Return one complete Microsoft SQL Server T-SQL query.");
        builder.AppendLine("- Return SQL only.");
        builder.AppendLine("- Do not include explanations.");
        builder.AppendLine("- Do not include markdown code fences.");
        builder.AppendLine("- Do not include comments.");
        builder.AppendLine("- Do not stop mid-query.");
        builder.AppendLine("- Ensure all parentheses, CTEs, joins, and window functions are complete.");
        builder.AppendLine("- If using ROW_NUMBER(), complete the full OVER (...) expression.");
        builder.AppendLine("- If using a CTE, include the final SELECT after the CTE.");

        return builder.ToString();
    }

    private static string BuildRetryPrompt(
        string originalPrompt)
    {
        var userRequest =
            ExtractUserRequest(originalPrompt);

        var metadataContext =
            ExtractMetadataContext(originalPrompt);

        var builder = new StringBuilder();

        builder.AppendLine("You are an Epicor Kinetic SQL expert.");
        builder.AppendLine("Generate exactly one complete Microsoft SQL Server T-SQL SELECT query.");
        builder.AppendLine("Return SQL only.");
        builder.AppendLine("Do not include explanations.");
        builder.AppendLine("Do not include markdown.");
        builder.AppendLine("Use TOP instead of LIMIT.");
        builder.AppendLine("Use only supplied Epicor tables and fields.");
        builder.AppendLine("Do not invent tables or columns.");
        builder.AppendLine("Ensure the SQL is complete and syntactically valid.");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(metadataContext))
        {
            builder.AppendLine("### METADATA CONTEXT ###");
            builder.AppendLine(metadataContext);
            builder.AppendLine();
        }

        builder.AppendLine("### USER REQUEST ###");
        builder.AppendLine(userRequest);
        builder.AppendLine();
        builder.AppendLine("### SQL ONLY ###");

        return builder.ToString();
    }

    private static string ExtractSql(
        string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return string.Empty;
        }

        var cleaned =
            response.Trim();

        cleaned =
            RemoveMarkdownCodeFence(cleaned);

        cleaned =
            RemoveCommonPrefixes(cleaned);

        cleaned =
            RemoveTrailingExplanation(cleaned);

        cleaned =
            cleaned.Trim();

        return cleaned;
    }

    private static string RemoveMarkdownCodeFence(
        string text)
    {
        var fencedSqlMatch =
            Regex.Match(
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

    private static string RemoveCommonPrefixes(
        string text)
    {
        var cleaned =
            text.Trim();

        var prefixes =
            new[]
            {
                "SQL:",
                "T-SQL:",
                "Generated SQL:",
                "Here is the SQL:",
                "Here is the T-SQL:",
                "The SQL is:",
                "Query:"
            };

        foreach (var prefix in prefixes)
        {
            if (!cleaned.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            cleaned =
                cleaned[prefix.Length..].Trim();

            break;
        }

        return cleaned;
    }

    private static string RemoveTrailingExplanation(
        string text)
    {
        var explanationMarkers =
            new[]
            {
                "\nExplanation:",
                "\nNotes:",
                "\nThis query",
                "\nThe query",
                "\nIt uses"
            };

        foreach (var marker in explanationMarkers)
        {
            var index =
                text.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                return text[..index].Trim();
            }
        }

        return text;
    }

    private static string ExtractUserRequest(
        string prompt)
    {
        var marker =
            "### USER REQUEST ###";

        var index =
            prompt.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return prompt.Trim();
        }

        var afterMarker =
            prompt[(index + marker.Length)..];

        var nextSectionIndex =
            afterMarker.IndexOf(
                "###",
                StringComparison.OrdinalIgnoreCase);

        if (nextSectionIndex >= 0)
        {
            return afterMarker[..nextSectionIndex].Trim();
        }

        return afterMarker.Trim();
    }

    private static string ExtractMetadataContext(
        string prompt)
    {
        var marker =
            "### EPICOR METADATA CONTEXT ###";

        var index =
            prompt.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return string.Empty;
        }

        var afterMarker =
            prompt[(index + marker.Length)..];

        var nextSectionIndex =
            afterMarker.IndexOf(
                "### USER REQUEST ###",
                StringComparison.OrdinalIgnoreCase);

        if (nextSectionIndex >= 0)
        {
            return afterMarker[..nextSectionIndex].Trim();
        }

        return afterMarker.Trim();
    }
}