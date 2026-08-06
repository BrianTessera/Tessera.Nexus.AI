using System.Text.RegularExpressions;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class SqlValidator : ISqlValidator
{
    private static readonly string[] ProhibitedKeywords =
    [
        "INSERT",
        "UPDATE",
        "DELETE",
        "MERGE",
        "TRUNCATE",
        "DROP",
        "ALTER",
        "CREATE",
        "EXEC",
        "EXECUTE",
        "GRANT",
        "REVOKE",
        "DENY",
        "BACKUP",
        "RESTORE",
        "DBCC",
        "XP_CMDSHELL",
        "SP_OACREATE"
    ];

    public bool IsValid(string sql)
    {
        return Validate(sql).IsValid;
    }

    public SqlValidationResult Validate(string sql)
    {
        var result = new SqlValidationResult();

        if (string.IsNullOrWhiteSpace(sql))
        {
            result.IsValid = false;
            result.ErrorMessage = "SQL statement is empty.";
            result.Violations.Add("SQL statement is empty.");

            return result;
        }

        var normalizedSql = NormalizeSql(sql);

        ValidateLeadingStatement(normalizedSql, result);

        ValidateProhibitedKeywords(normalizedSql, result);

        ValidateMultiStatement(normalizedSql, result);

        ValidateSelectStar(normalizedSql, result);

        result.IsValid = result.Violations.Count == 0;

        if (!result.IsValid && string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            result.ErrorMessage =
                string.Join(Environment.NewLine, result.Violations);
        }

        return result;
    }

    private static void ValidateLeadingStatement(
        string sql,
        SqlValidationResult result)
    {
        if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        result.Violations.Add(
            "SQL must begin with SELECT or WITH.");

        result.ErrorMessage =
            "Only read-only SELECT statements are allowed.";
    }

    private static void ValidateProhibitedKeywords(
        string sql,
        SqlValidationResult result)
    {
        foreach (var keyword in ProhibitedKeywords)
        {
            var pattern = $@"\b{Regex.Escape(keyword)}\b";

            if (Regex.IsMatch(
                sql,
                pattern,
                RegexOptions.IgnoreCase))
            {
                result.Violations.Add(
                    $"Prohibited keyword detected: {keyword}");
            }
        }
    }

    private static void ValidateMultiStatement(
        string sql,
        SqlValidationResult result)
    {
        var statementCount =
            sql.Count(character => character == ';');

        if (statementCount > 1)
        {
            result.Violations.Add(
                "Multiple SQL statements are not allowed.");
        }
    }

    private static void ValidateSelectStar(
        string sql,
        SqlValidationResult result)
    {
        if (Regex.IsMatch(
            sql,
            @"SELECT\s+\*",
            RegexOptions.IgnoreCase))
        {
            result.Warnings.Add(
                "SELECT * detected. Explicit column lists are recommended.");
        }
    }

    private static string NormalizeSql(string sql)
    {
        return sql
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }
}