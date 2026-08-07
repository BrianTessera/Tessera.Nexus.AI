using System.Text;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class MetadataContextBuilder : IMetadataContextBuilder
{
    private readonly IEpicorMetadataRepository _metadataRepository;

    public MetadataContextBuilder(
        IEpicorMetadataRepository metadataRepository,
        IRelationshipContextBuilder relationshipContextBuilder)
    {
        _metadataRepository = metadataRepository;
        _relationshipContextBuilder = relationshipContextBuilder;
    }
    private readonly IRelationshipContextBuilder _relationshipContextBuilder;

    public async Task<string> BuildMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return string.Empty;
        }

        var metadata =
            await _metadataRepository.GetMetadataContextAsync(
                userQuestion,
                cancellationToken);

        if (metadata.Tables.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        builder.AppendLine("### EPICOR METADATA CONTEXT ###");
        builder.AppendLine();

        builder.AppendLine(
            "Use only the supplied Epicor metadata context when generating SQL.");

        builder.AppendLine(
            "Do not invent table names, column names, relationships, or filters.");

        builder.AppendLine(
            "Generate Microsoft SQL Server T-SQL.");

        builder.AppendLine(
            "Use TOP instead of LIMIT.");

        builder.AppendLine();

        AppendRelevantTables(
            builder,
            metadata.Tables);

        AppendRelevantFields(
            builder,
            metadata.Fields);

        var relationshipContext =
            _relationshipContextBuilder.BuildRelationshipContext(
                metadata.Relationships);

        if (!string.IsNullOrWhiteSpace(relationshipContext))
        {
            builder.AppendLine(relationshipContext);
        }

        AppendBusinessSearchHints(
            builder,
            metadata.Tables,
            metadata.Fields,
            userQuestion);

        AppendPrimaryKeyHints(
            builder,
            metadata.Fields);

        AppendCommonFilterHints(
            builder,
            metadata.Fields);

        AppendEntityRecognitionHints(
            builder,
            userQuestion);

        AppendSqlRules(
            builder);

        return builder.ToString();
    }

    private static void AppendRelevantTables(
        StringBuilder builder,
        IReadOnlyList<EpicorDataTable> tables)
    {
        builder.AppendLine("Relevant Tables");
        builder.AppendLine();

        foreach (var table in tables
                     .OrderBy(table => table.SchemaName)
                     .ThenBy(table => table.DbTableName))
        {
            builder.AppendLine(
                $"- {table.SchemaName}.{table.DbTableName}");
        }

        builder.AppendLine();
    }

    private static void AppendRelevantFields(
        StringBuilder builder,
        IReadOnlyList<EpicorDataField> fields)
    {
        var groupedFields =
            fields
                .GroupBy(field => field.DataTableId)
                .OrderBy(group => group.Key);

        foreach (var group in groupedFields)
        {
            builder.AppendLine(
                $"Table: {group.Key}");

            foreach (var field in group
                         .OrderBy(field => field.FieldName))
            {
                builder.AppendLine(
                    $"  - {field.FieldName}");
            }

            builder.AppendLine();
        }
    }

    private static void AppendRelationships(
        StringBuilder builder,
        IReadOnlyList<EpicorRelation> relationships)
    {
        if (relationships.Count == 0)
        {
            return;
        }

        builder.AppendLine("Relationships");
        builder.AppendLine();

        foreach (var relationship in relationships
                     .OrderBy(relationship => relationship.RelationId))
        {
            builder.AppendLine(
                $"- {relationship.RelationId}");
        }

        builder.AppendLine();
    }

    private static void AppendBusinessSearchHints(
        StringBuilder builder,
        IReadOnlyList<EpicorDataTable> tables,
        IReadOnlyList<EpicorDataField> fields,
        string userQuestion)
    {
        builder.AppendLine("Business Search Hints");
        builder.AppendLine();

        if (ContainsTable(
                tables,
                "ShipHead"))
        {
            builder.AppendLine(
                "- Shipment number usually refers to ShipHead.PackNum.");

            builder.AppendLine(
                "- Ship date usually refers to ShipHead.ShipDate.");
        }

        if (ContainsTable(
                tables,
                "ShipDtl"))
        {
            builder.AppendLine(
                "- Shipment value is often derived from ShipDtl.DocExtPrice.");

            builder.AppendLine(
                "- Shipment quantity is often derived from ShipDtl.SellingInventoryShipQty.");
        }

        if (ContainsCustomerLikeReference(
                userQuestion) ||
            ContainsCustomerMetadata(
                tables,
                fields))
        {
            builder.AppendLine(
                "- Customer.CustNum is the numeric internal Epicor customer key.");

            builder.AppendLine(
                "- Customer.CustID is the text business customer code.");

            builder.AppendLine(
                "- Values such as NGC are Customer.CustID values, not Customer.CustNum values.");

            builder.AppendLine(
                "- Customer.Name is the customer display name.");

            builder.AppendLine(
                "- Join ShipHead to Customer using Company and CustNum.");

            builder.AppendLine(
                "- The correct customer join is ShipHead.Company = Customer.Company AND ShipHead.CustNum = Customer.CustNum.");

            builder.AppendLine(
                "- Never join ShipHead.CustNum to Customer.CustID.");

            builder.AppendLine(
                "- Never compare Customer.CustNum to a quoted text customer code.");
        }

        if (ContainsField(
                fields,
                "CustID"))
        {
            builder.AppendLine(
                "- Filter Customer.CustID when the user provides a known customer code.");
        }

        if (ContainsField(
                fields,
                "Name"))
        {
            builder.AppendLine(
                "- Filter Customer.Name when the user provides a customer name.");
        }

        if (ContainsCustomerLikeReference(
                userQuestion))
        {
            builder.AppendLine(
                "- A named organization such as Boeing, Northrop, Lockheed, Raytheon, or L3Harris should usually be treated as a customer search.");

            builder.AppendLine(
                "- Prefer Customer.Name LIKE '%value%' for partial customer-name searches.");

            builder.AppendLine(
                "- Prefer Customer.CustID = 'value' when an exact customer code is known.");

            builder.AppendLine(
                "- Do not assume ShipToNum contains a customer name.");
        }

        builder.AppendLine();
    }

    private static void AppendPrimaryKeyHints(
        StringBuilder builder,
        IReadOnlyList<EpicorDataField> fields)
    {
        builder.AppendLine("Primary Key and Identifier Hints");
        builder.AppendLine();

        if (ContainsField(
                fields,
                "Company"))
        {
            builder.AppendLine(
                "- Company is commonly part of Epicor primary keys and joins.");
        }

        if (ContainsField(
                fields,
                "CustNum"))
        {
            builder.AppendLine(
                "- CustNum is the numeric internal customer key used in Epicor relationships.");
        }

        if (ContainsField(
                fields,
                "CustID"))
        {
            builder.AppendLine(
                "- CustID is a text business identifier and must not be used as a numeric join key.");
        }

        if (ContainsField(
                fields,
                "PackNum"))
        {
            builder.AppendLine(
                "- PackNum is typically the shipment key.");
        }

        if (ContainsField(
                fields,
                "PartNum"))
        {
            builder.AppendLine(
                "- PartNum is typically the part key.");
        }

        if (ContainsField(
                fields,
                "JobNum"))
        {
            builder.AppendLine(
                "- JobNum is typically the job key.");
        }

        builder.AppendLine();
    }

    private static void AppendCommonFilterHints(
        StringBuilder builder,
        IReadOnlyList<EpicorDataField> fields)
    {
        builder.AppendLine("Common Filter Hints");
        builder.AppendLine();

        if (ContainsField(
                fields,
                "Name"))
        {
            builder.AppendLine(
                "- Customer names should generally use Customer.Name LIKE '%value%' rather than exact equality.");
        }

        if (ContainsField(
                fields,
                "CustID"))
        {
            builder.AppendLine(
                "- Customer code searches should use Customer.CustID with a quoted text value.");

            builder.AppendLine(
                "- Example customer-code filter: Customer.CustID = 'NGC'.");
        }

        if (ContainsField(
                fields,
                "CustNum"))
        {
            builder.AppendLine(
                "- Customer.CustNum is numeric and must only be compared to numeric customer keys.");

            builder.AppendLine(
                "- Do not filter CustNum using values such as 'NGC', 'Northrop', or 'Boeing'.");
        }

        if (ContainsField(
                fields,
                "ShipDate"))
        {
            builder.AppendLine(
                "- Shipment reporting should generally filter and sort by ShipHead.ShipDate.");
        }

        if (ContainsField(
                fields,
                "PackNum"))
        {
            builder.AppendLine(
                "- Shipment lookups typically use ShipHead.PackNum.");
        }

        builder.AppendLine();
    }

    private static void AppendEntityRecognitionHints(
        StringBuilder builder,
        string userQuestion)
    {
        builder.AppendLine("Entity Recognition Hints");
        builder.AppendLine();

        if (ContainsCustomerLikeReference(
                userQuestion))
        {
            builder.AppendLine(
                "- The user question appears to reference a customer or organization by name.");

            builder.AppendLine(
                "- Include Customer metadata and joins when customer names are referenced.");

            builder.AppendLine(
                "- Customer name matching should prefer LIKE searches rather than exact matches.");

            builder.AppendLine(
                "- A customer name and a customer code are different identifiers.");

            builder.AppendLine(
                "- Treat names such as Northrop or Boeing Commercial as partial Customer.Name values unless an exact CustID is explicitly supplied.");
        }

        builder.AppendLine();
    }

    private static void AppendSqlRules(
        StringBuilder builder)
    {
        builder.AppendLine("SQL Rules");
        builder.AppendLine();

        builder.AppendLine(
            "- Use Microsoft SQL Server T-SQL syntax.");

        builder.AppendLine(
            "- Use TOP instead of LIMIT.");

        builder.AppendLine(
            "- Generate one complete read-only SELECT statement or one complete CTE followed by a SELECT statement.");

        builder.AppendLine(
            "- Do not create tables.");

        builder.AppendLine(
            "- Do not alter tables.");

        builder.AppendLine(
            "- Do not insert records.");

        builder.AppendLine(
            "- Do not update records.");

        builder.AppendLine(
            "- Do not delete records.");

        builder.AppendLine(
            "- Do not merge records.");

        builder.AppendLine(
            "- Do not execute stored procedures.");

        builder.AppendLine(
            "- Do not invent table names, column names, relationships, or filters.");

        builder.AppendLine(
            "- Use only the metadata and approved guidance supplied above.");

        builder.AppendLine(
            "- Preserve data-type compatibility in every join and filter.");

        builder.AppendLine(
            "- Prefer Company-aware joins when joining Epicor tables.");

        builder.AppendLine(
            "- Join ShipHead to Customer using ShipHead.Company = Customer.Company AND ShipHead.CustNum = Customer.CustNum.");

        builder.AppendLine(
            "- Never join ShipHead.CustNum to Customer.CustID.");

        builder.AppendLine(
            "- CustNum is numeric and must only be compared with numeric customer keys.");

        builder.AppendLine(
            "- CustID is text and must only be compared with quoted text customer codes.");

        builder.AppendLine(
            "- When filtering by a partial customer name, use Customer.Name LIKE '%value%'.");

        builder.AppendLine(
            "- When filtering by an exact customer code, use Customer.CustID = 'value'.");

        builder.AppendLine(
            "- For shipment value calculations, prefer ShipDtl.DocExtPrice when that field is available.");

        builder.AppendLine(
            "- When joining ShipHead to ShipDtl, use Company and PackNum.");

        builder.AppendLine(
            "- Add deterministic secondary ordering when returning TOP shipment rows, such as PackNum DESC after ShipDate DESC.");

        builder.AppendLine(
            "- Complete all parentheses, joins, CTEs, window functions, and ORDER BY clauses.");

        builder.AppendLine();
    }

    private static bool ContainsCustomerLikeReference(
        string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(
                userQuestion))
        {
            return false;
        }

        var normalized =
            userQuestion.ToUpperInvariant();

        return normalized.Contains("BOEING")
               || normalized.Contains("NORTHROP")
               || normalized.Contains("LOCKHEED")
               || normalized.Contains("RAYTHEON")
               || normalized.Contains("L3HARRIS")
               || normalized.Contains("CUSTOMER")
               || normalized.Contains("CUSTID")
               || normalized.Contains("CUST ID")
               || normalized.Contains("CUSTOMER CODE");
    }

    private static bool ContainsCustomerMetadata(
        IEnumerable<EpicorDataTable> tables,
        IEnumerable<EpicorDataField> fields)
    {
        return ContainsTable(
                   tables,
                   "Customer")
               || ContainsField(
                   fields,
                   "CustNum")
               || ContainsField(
                   fields,
                   "CustID");
    }

    private static bool ContainsTable(
        IEnumerable<EpicorDataTable> tables,
        string tableName)
    {
        return tables.Any(
            table =>
                string.Equals(
                    table.DbTableName,
                    tableName,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsField(
        IEnumerable<EpicorDataField> fields,
        string fieldName)
    {
        return fields.Any(
            field =>
                string.Equals(
                    field.FieldName,
                    fieldName,
                    StringComparison.OrdinalIgnoreCase));
    }
}