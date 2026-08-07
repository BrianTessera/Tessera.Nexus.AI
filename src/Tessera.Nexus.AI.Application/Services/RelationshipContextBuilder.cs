using System.Collections;
using System.Reflection;
using System.Text;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class RelationshipContextBuilder : IRelationshipContextBuilder
{
    public string BuildRelationshipContext(
        IReadOnlyList<EpicorRelation> relationships)
    {
        if (relationships.Count == 0)
        {
            return string.Empty;
        }

        var builder =
            new StringBuilder();

        builder.AppendLine("### APPROVED EPICOR RELATIONSHIPS ###");
        builder.AppendLine();

        builder.AppendLine(
            "Use only these approved Epicor relationship definitions when joining the listed tables.");

        builder.AppendLine(
            "Do not invent joins when an approved relationship is provided.");

        builder.AppendLine(
            "When relationship field mappings are available, use the listed field mappings exactly.");

        builder.AppendLine();

        foreach (var relationship in relationships
                     .OrderBy(GetParentTableName)
                     .ThenBy(GetChildTableName)
                     .ThenBy(GetRelationId))
        {
            AppendRelationship(
                builder,
                relationship);
        }

        builder.AppendLine("Relationship Rules");
        builder.AppendLine("- Prefer approved relationship field mappings over inferred joins.");
        builder.AppendLine("- Preserve data-type compatibility across all joins.");
        builder.AppendLine("- Include Company in joins when Company is part of the approved relationship.");
        builder.AppendLine("- Do not join CustNum to CustID unless an approved relationship explicitly states that mapping.");
        builder.AppendLine("- CustNum is normally a numeric internal key.");
        builder.AppendLine("- CustID is normally a text business identifier.");
        builder.AppendLine();

        return builder.ToString();
    }

    private static void AppendRelationship(
        StringBuilder builder,
        EpicorRelation relationship)
    {
        var relationId =
            GetRelationId(
                relationship);

        var parentTable =
            GetParentTableName(
                relationship);

        var childTable =
            GetChildTableName(
                relationship);

        var description =
            ReadString(
                relationship,
                "Description");

        builder.AppendLine(
            $"- Relationship: {relationId}");

        if (!string.IsNullOrWhiteSpace(parentTable) ||
            !string.IsNullOrWhiteSpace(childTable))
        {
            builder.AppendLine(
                $"  Tables: {Display(parentTable)} -> {Display(childTable)}");
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            builder.AppendLine(
                $"  Description: {description}");
        }

        var fields =
            GetRelationshipFields(
                relationship);

        if (fields.Count == 0)
        {
            builder.AppendLine(
                "  Join Fields: Field-level mappings were not available in the relationship object.");

            builder.AppendLine(
                "  Instruction: Do not infer field mappings from this relationship name alone.");

            builder.AppendLine();

            return;
        }

        builder.AppendLine(
            "  Join Fields:");

        foreach (var field in fields
                     .OrderBy(GetSequence)
                     .ThenBy(GetParentFieldName)
                     .ThenBy(GetChildFieldName))
        {
            AppendRelationshipField(
                builder,
                field,
                parentTable,
                childTable);
        }

        builder.AppendLine();
    }

    private static void AppendRelationshipField(
        StringBuilder builder,
        object field,
        string? parentTable,
        string? childTable)
    {
        var parentField =
            GetParentFieldName(
                field);

        var childField =
            GetChildFieldName(
                field);

        var comparison =
            ReadString(
                field,
                "CompOp",
                "ComparisonOperator",
                "Operator");

        if (string.IsNullOrWhiteSpace(comparison))
        {
            comparison = "=";
        }

        var isConstant =
            ReadBool(
                field,
                "IsConst",
                "IsConstant");

        if (isConstant)
        {
            builder.AppendLine(
                $"    - Constant {Display(parentField)} {comparison} {Display(childTable)}.{Display(childField)}");

            return;
        }

        builder.AppendLine(
            $"    - {Display(parentTable)}.{Display(parentField)} {comparison} {Display(childTable)}.{Display(childField)}");
    }

    private static IReadOnlyList<object> GetRelationshipFields(
        object relationship)
    {
        var possibleNames =
            new[]
            {
                "Fields",
                "RelationFields",
                "RelationshipFields",
                "FieldMappings",
                "Mappings"
            };

        foreach (var name in possibleNames)
        {
            var property =
                relationship
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(
                        candidate =>
                            string.Equals(
                                candidate.Name,
                                name,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(
                    relationship);

            if (value is null ||
                value is string)
            {
                continue;
            }

            if (value is not IEnumerable enumerable)
            {
                continue;
            }

            return enumerable
                .Cast<object>()
                .ToList();
        }

        return Array.Empty<object>();
    }

    private static string GetRelationId(
        object relationship)
    {
        return ReadString(
                   relationship,
                   "RelationId",
                   "RelationID",
                   "RelationshipId",
                   "RelationshipID") ??
               "UnknownRelationship";
    }

    private static string? GetParentTableName(
        object relationship)
    {
        return ReadString(
            relationship,
            "ParentDataTableID",
            "ParentDataTableId",
            "ParentTableName",
            "ParentTable",
            "ParentObjectName");
    }

    private static string? GetChildTableName(
        object relationship)
    {
        return ReadString(
            relationship,
            "ChildDataTableID",
            "ChildDataTableId",
            "ChildTableName",
            "ChildTable",
            "ChildObjectName");
    }

    private static string? GetParentFieldName(
        object field)
    {
        return ReadString(
            field,
            "ParentFieldName",
            "ParentField",
            "ParentColumnName",
            "ParentColumn");
    }

    private static string? GetChildFieldName(
        object field)
    {
        return ReadString(
            field,
            "ChildFieldName",
            "ChildField",
            "ChildColumnName",
            "ChildColumn");
    }

    private static int GetSequence(
        object field)
    {
        return ReadInt(
                   field,
                   "Seq",
                   "Sequence",
                   "SequenceNumber",
                   "SortOrder") ??
               0;
    }

    private static string? ReadString(
        object source,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property =
                source
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(
                        candidate =>
                            string.Equals(
                                candidate.Name,
                                propertyName,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(
                    source);

            if (value is null)
            {
                continue;
            }

            var text =
                Convert.ToString(
                    value);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
        }

        return null;
    }

    private static int? ReadInt(
        object source,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property =
                source
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(
                        candidate =>
                            string.Equals(
                                candidate.Name,
                                propertyName,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(
                    source);

            if (value is null)
            {
                continue;
            }

            if (value is int integerValue)
            {
                return integerValue;
            }

            if (int.TryParse(
                    Convert.ToString(value),
                    out var parsedValue))
            {
                return parsedValue;
            }
        }

        return null;
    }

    private static bool ReadBool(
        object source,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property =
                source
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(
                        candidate =>
                            string.Equals(
                                candidate.Name,
                                propertyName,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(
                    source);

            if (value is null)
            {
                continue;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            var text =
                Convert.ToString(
                    value);

            if (string.Equals(
                    text,
                    "1",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(
                    text,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Display(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unknown"
            : value.Trim();
    }
}