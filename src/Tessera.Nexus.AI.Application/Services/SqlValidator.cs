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

    private static readonly string[] IncompleteTrailingPatterns =
    [
        @"\bWHERE\s*$",
        @"\bAND\s*$",
        @"\bOR\s*$",
        @"\bFROM\s*$",
        @"\bJOIN\s*$",
        @"\bINNER\s+JOIN\s*$",
        @"\bLEFT\s+JOIN\s*$",
        @"\bRIGHT\s+JOIN\s*$",
        @"\bFULL\s+JOIN\s*$",
        @"\bON\s*$",
        @"\bGROUP\s+BY\s*$",
        @"\bORDER\s+BY\s*$",
        @"\bPARTITION\s+BY\s*$",
        @"\bOVER\s*$",
        @",\s*$"
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

        var sqlWithoutStringLiterals =
            RemoveStringLiterals(normalizedSql);

        ValidateLeadingStatement(
            normalizedSql,
            result);

        ValidateBalancedParentheses(
            sqlWithoutStringLiterals,
            result);

        ValidateIncompleteTrailingSql(
            sqlWithoutStringLiterals,
            result);

        ValidateIncompleteWindowFunction(
            sqlWithoutStringLiterals,
            result);

        ValidateCteStructure(
            sqlWithoutStringLiterals,
            result);

        ValidateProhibitedKeywords(
            sqlWithoutStringLiterals,
            result);

        ValidateMultiStatement(
            sqlWithoutStringLiterals,
            result);

        ValidateSelectStar(
            sqlWithoutStringLiterals,
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

        ValidateUnionOrderBy(
            sqlWithoutStringLiterals,
            result);

        ValidateTopOrderByUnionPattern(
            sqlWithoutStringLiterals,
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

    private static void ValidateBalancedParentheses(
        string sql,
        SqlValidationResult result)
    {
        var balance = 0;

        foreach (var character in sql)
        {
            if (character == '(')
            {
                balance++;
                continue;
            }

            if (character == ')')
            {
                balance--;
            }

            if (balance >= 0)
            {
                continue;
            }

            result.Violations.Add(
                "SQL contains an unmatched closing parenthesis.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "SQL contains unbalanced parentheses.";
            }

            return;
        }

        if (balance == 0)
        {
            return;
        }

        result.Violations.Add(
            "SQL contains unbalanced parentheses. The statement may be incomplete.");

        if (string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            result.ErrorMessage =
                "SQL contains unbalanced parentheses.";
        }
    }

    private static void ValidateIncompleteTrailingSql(
        string sql,
        SqlValidationResult result)
    {
        foreach (var pattern in IncompleteTrailingPatterns)
        {
            if (!Regex.IsMatch(
                    sql,
                    pattern,
                    RegexOptions.IgnoreCase))
            {
                continue;
            }

            result.Violations.Add(
                "SQL appears to end with an incomplete clause or expression.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "Incomplete SQL statement detected.";
            }

            return;
        }
    }

    private static void ValidateIncompleteWindowFunction(
        string sql,
        SqlValidationResult result)
    {
        if (Regex.IsMatch(
                sql,
                @"\bOVER\s*\([^)]*$",
                RegexOptions.IgnoreCase))
        {
            result.Violations.Add(
                "Incomplete OVER clause detected. Window functions must include a complete OVER (...) expression.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "Incomplete SQL window function detected.";
            }
        }

        if (Regex.IsMatch(
                sql,
                @"\bROW_NUMBER\s*\(\s*\)\s*OVER\s*\([^)]*$",
                RegexOptions.IgnoreCase))
        {
            result.Violations.Add(
                "Incomplete ROW_NUMBER OVER clause detected.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "Incomplete ROW_NUMBER expression detected.";
            }
        }
    }

    private static void ValidateCteStructure(
        string sql,
        SqlValidationResult result)
    {
        if (!sql.StartsWith(
                "WITH",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!Regex.IsMatch(
                sql,
                @"\bWITH\b.+\bAS\s*\(",
                RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            result.Violations.Add(
                "CTE query starts with WITH but does not contain a valid AS (...) definition.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "Invalid CTE structure detected.";
            }

            return;
        }

        if (!Regex.IsMatch(
                sql,
                @"\)\s*SELECT\b",
                RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            result.Violations.Add(
                "CTE query appears incomplete. A WITH clause must be followed by an outer SELECT statement.");

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                result.ErrorMessage =
                    "Incomplete CTE query detected.";
            }
        }
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
        var trimmed =
            sql.Trim();

        var semicolonCount =
            trimmed.Count(character => character == ';');

        if (semicolonCount == 0)
        {
            return;
        }

        if (semicolonCount == 1 &&
            trimmed.EndsWith(
                ";",
                StringComparison.Ordinal))
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
                @"\bc\.Name\s*=\s*'[^']+'",
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
                @"PackNum\s+LIKE",
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

    private static void ValidateUnionOrderBy(
        string sql,
        SqlValidationResult result)
    {
        var containsUnion =
            Regex.IsMatch(
                sql,
                @"\bUNION\b|\bUNION\s+ALL\b",
                RegexOptions.IgnoreCase);

        if (!containsUnion)
        {
            return;
        }

        var orderByCount =
            Regex.Matches(
                    sql,
                    @"\bORDER\s+BY\b",
                    RegexOptions.IgnoreCase)
                .Count;

        if (orderByCount <= 1)
        {
            return;
        }

        result.Violations.Add(
            """
            Multiple ORDER BY clauses detected in a UNION query.
            SQL Server requires ORDER BY to appear only once
            at the outermost query level unless each SELECT
            is wrapped inside a derived table or CTE.
            """);

        if (string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            result.ErrorMessage =
                "Invalid SQL Server UNION ORDER BY pattern detected.";
        }
    }

    private static void ValidateTopOrderByUnionPattern(
        string sql,
        SqlValidationResult result)
    {
        var containsUnion =
            Regex.IsMatch(
                sql,
                @"\bUNION\b|\bUNION\s+ALL\b",
                RegexOptions.IgnoreCase);

        if (!containsUnion)
        {
            return;
        }

        var containsTop =
            Regex.IsMatch(
                sql,
                @"\bTOP\s+\(?\d+\)?",
                RegexOptions.IgnoreCase);

        if (!containsTop)
        {
            return;
        }

        result.Warnings.Add(
            """
            UNION query contains TOP clauses.
            Consider using derived tables or CTEs
            to preserve ordering within each TOP query.
            """);
    }

    private static string NormalizeSql(
        string sql)
    {
        return sql
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }

    private static string RemoveStringLiterals(
        string sql)
    {
        return Regex.Replace(
            sql,
            @"'([^']|'')*'",
            "''",
            RegexOptions.Singleline);
    }
}