using System.Text;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class MetadataContextBuilder : IMetadataContextBuilder
{
    private readonly IEpicorMetadataRepository _metadataRepository;

    public MetadataContextBuilder(
        IEpicorMetadataRepository metadataRepository)
    {
        _metadataRepository = metadataRepository;
    }

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

        AppendRelationships(
            builder,
            metadata.Relationships);

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

        AppendSqlRules(builder);

        return builder.ToString();
    }

    private static void AppendRelevantTables(
        StringBuilder builder,
        IReadOnlyList<EpicorDataTable> tables)
    {
        builder.AppendLine("Relevant Tables");
        builder.AppendLine();

        foreach (var table in tables
                     .OrderBy(t => t.SchemaName)
                     .ThenBy(t => t.DbTableName))
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

        foreach (var relationship in relationships)
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

        builder.AppendLine(
            "- Shipment number usually refers to ShipHead.PackNum.");

        builder.AppendLine(
            "- Ship date usually refers to ShipHead.ShipDate.");

        builder.AppendLine(
            "- Shipment value is often derived from ShipDtl.DocExtPrice.");

        builder.AppendLine(
            "- Shipment quantity is often derived from ShipDtl.SellingInventoryShipQty.");

        if (ContainsField(fields, "CustID"))
        {
            builder.AppendLine(
                "- Customer codes typically use Customer.CustID.");
        }

        if (ContainsField(fields, "Name"))
        {
            builder.AppendLine(
                "- Customer name searches typically use Customer.Name.");
        }

        if (ContainsCustomerLikeReference(userQuestion))
        {
            builder.AppendLine(
                "- A named organization (for example Boeing, Northrop, Lockheed, Raytheon) should usually be treated as a customer search.");

            builder.AppendLine(
                "- Prefer Customer.Name LIKE '%value%' for customer name searches.");

            builder.AppendLine(
                "- Prefer Customer.CustID when a customer code is known.");

            builder.AppendLine(
                "- Do not assume ShipToNum contains a customer name.");
        }

        builder.AppendLine();
    }

    private static void AppendPrimaryKeyHints(
        StringBuilder builder,
        IReadOnlyList<EpicorDataField> fields)
    {
        builder.AppendLine("Primary Key Hints");
        builder.AppendLine();

        if (ContainsField(fields, "Company"))
        {
            builder.AppendLine(
                "- Company is commonly part of Epicor joins.");
        }

        if (ContainsField(fields, "CustNum"))
        {
            builder.AppendLine(
                "- CustNum is typically the customer key.");
        }

        if (ContainsField(fields, "PackNum"))
        {
            builder.AppendLine(
                "- PackNum is typically the shipment key.");
        }

        if (ContainsField(fields, "PartNum"))
        {
            builder.AppendLine(
                "- PartNum is typically the part key.");
        }

        if (ContainsField(fields, "JobNum"))
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

        if (ContainsField(fields, "Name"))
        {
            builder.AppendLine(
                "- Customer names should generally use LIKE '%value%' rather than exact equality.");
        }

        if (ContainsField(fields, "CustID"))
        {
            builder.AppendLine(
                "- Customer code searches should generally use Customer.CustID.");
        }

        if (ContainsField(fields, "ShipDate"))
        {
            builder.AppendLine(
                "- Shipment reporting should generally filter and sort by ShipDate.");
        }

        if (ContainsField(fields, "PackNum"))
        {
            builder.AppendLine(
                "- Shipment lookups typically use PackNum.");
        }

        builder.AppendLine();
    }

    private static void AppendEntityRecognitionHints(
        StringBuilder builder,
        string userQuestion)
    {
        builder.AppendLine("Entity Recognition Hints");
        builder.AppendLine();

        if (ContainsCustomerLikeReference(userQuestion))
        {
            builder.AppendLine(
                "- The user question appears to reference a customer or organization by name.");

            builder.AppendLine(
                "- Include Customer metadata and joins when customer names are referenced.");

            builder.AppendLine(
                "- Customer name matching should prefer LIKE searches rather than exact matches.");
        }

        builder.AppendLine();
    }

    private static void AppendSqlRules(
        StringBuilder builder)
    {
        builder.AppendLine("SQL Rules");
        builder.AppendLine();

        builder.AppendLine(
            "- Use SQL Server syntax.");

        builder.AppendLine(
            "- Use TOP instead of LIMIT.");

        builder.AppendLine(
            "- Do not create tables.");

        builder.AppendLine(
            "- Do not alter tables.");

        builder.AppendLine(
            "- Do not invent column names.");

        builder.AppendLine(
            "- Prefer metadata supplied above.");

        builder.AppendLine(
            "- Prefer Company-aware joins when joining Epicor tables.");

        builder.AppendLine(
            "- When filtering by a customer name, prefer Customer.Name LIKE '%value%'.");

        builder.AppendLine(
            "- When filtering by a customer code, prefer Customer.CustID.");

        builder.AppendLine(
            "- For shipment value calculations, prefer ShipDtl.DocExtPrice.");

        builder.AppendLine();
    }

    private static bool ContainsCustomerLikeReference(
        string userQuestion)
    {
        var normalized =
            userQuestion.ToUpperInvariant();

        return normalized.Contains("BOEING")
               || normalized.Contains("NORTHROP")
               || normalized.Contains("LOCKHEED")
               || normalized.Contains("RAYTHEON")
               || normalized.Contains("L3HARRIS")
               || normalized.Contains("CUSTOMER");
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