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

        var normalizedSql =
            NormalizeSql(sql);

        ValidateLeadingStatement(
            normalizedSql,
            result);

        ValidateProhibitedKeywords(
            normalizedSql,
            result);

        ValidateMultiStatement(
            normalizedSql,
            result);

        ValidateSelectStar(
            normalizedSql,
            result);

        ValidateCustomerEqualityFilter(
            normalizedSql,
            result);

        ValidateShipmentFilterHints(
            normalizedSql,
            result);

        ValidateCompanyJoinHints(
            normalizedSql,
            result);

        ValidateLiteralStringFilters(
            normalizedSql,
            result);

        result.IsValid =
            result.Violations.Count == 0;

        if (!result.IsValid &&
            string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            result.ErrorMessage =
                string.Join(
                    Environment.NewLine,
                    result.Violations);
        }

        return result;
    }

    private static void ValidateLeadingStatement(
        string sql,
        SqlValidationResult result)
    {
        if (sql.StartsWith(
                "SELECT",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (sql.StartsWith(
                "WITH",
                StringComparison.OrdinalIgnoreCase))
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
            var pattern =
                $@"\b{Regex.Escape(keyword)}\b";

            if (!Regex.IsMatch(
                    sql,
                    pattern,
                    RegexOptions.IgnoreCase))
            {
                continue;
            }

            result.Violations.Add(
                $"Prohibited keyword detected: {keyword}");
        }
    }

    private static void ValidateMultiStatement(
        string sql,
        SqlValidationResult result)
    {
        var statementCount =
            sql.Count(character => character == ';');

        if (statementCount <= 1)
        {
            return;
        }

        result.Violations.Add(
            "Multiple SQL statements are not allowed.");
    }

    private static void ValidateSelectStar(
        string sql,
        SqlValidationResult result)
    {
        if (!Regex.IsMatch(
                sql,
                @"SELECT\s+\*",
                RegexOptions.IgnoreCase))
        {
            return;
        }

        result.Warnings.Add(
            "SELECT * detected. Explicit column lists are recommended.");
    }

    private static void ValidateCustomerEqualityFilter(
        string sql,
        SqlValidationResult result)
    {
        var customerNameEqualityPatterns =
            new[]
            {
                @"Customer\.Name\s*=\s*'[^']+'",
                @"\bName\s*=\s*'[^']+'"
            };

        foreach (var pattern in customerNameEqualityPatterns)
        {
            if (!Regex.IsMatch(
                    sql,
                    pattern,
                    RegexOptions.IgnoreCase))
            {
                continue;
            }

            result.Warnings.Add(
                """
                Customer name equality filter detected.
                Consider using LIKE '%value%'
                because customer names are often partial matches.
                """);

            return;
        }
    }

    private static void ValidateShipmentFilterHints(
        string sql,
        SqlValidationResult result)
    {
        if (Regex.IsMatch(
                sql,
                @"ShipToNum\s*=\s*'[^']+'",
                RegexOptions.IgnoreCase))
        {
            result.Warnings.Add(
                """
                ShipToNum appears to be filtered using a literal name.
                Verify the value is truly a ShipToNum and not a customer name.
                Consider Customer.Name or Customer.CustID instead.
                """);
        }

        if (Regex.IsMatch(
                sql,
                @"PackNum\s*LIKE",
                RegexOptions.IgnoreCase))
        {
            result.Warnings.Add(
                """
                Shipment numbers are normally exact values.
                Consider PackNum = value instead of LIKE.
                """);
        }
    }

    private static void ValidateCompanyJoinHints(
        string sql,
        SqlValidationResult result)
    {
        var containsCustomerJoin =
            Regex.IsMatch(
                sql,
                @"JOIN\s+.*Customer",
                RegexOptions.IgnoreCase);

        if (!containsCustomerJoin)
        {
            return;
        }

        var containsCompanyJoin =
            Regex.IsMatch(
                sql,
                @"Company\s*=\s*.*Company",
                RegexOptions.IgnoreCase);

        if (containsCompanyJoin)
        {
            return;
        }

        result.Warnings.Add(
            """
            Customer join does not appear to include Company.
            Epicor joins should typically include Company
            in addition to CustNum.
            """);
    }

    private static void ValidateLiteralStringFilters(
        string sql,
        SqlValidationResult result)
    {
        var matches =
            Regex.Matches(
                sql,
                @"=\s*'([^']+)'",
                RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var value =
                match.Groups[1].Value;

            if (value.Length < 4)
            {
                continue;
            }

            result.Warnings.Add(
                $"Literal text filter detected: '{value}'. Verify exact match is intended.");
        }
    }

    private static string NormalizeSql(
        string sql)
    {
        return sql
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }
}