using System.Text;
using Tessera.Nexus.AI.Application.Contracts;

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

        var builder = new StringBuilder();

        builder.AppendLine("### EPICOR METADATA CONTEXT ###");
        builder.AppendLine();
        builder.AppendLine(
            "Use only the supplied Epicor metadata context when generating SQL.");
        builder.AppendLine(
            "Do not invent table names, column names, or relationships.");
        builder.AppendLine(
            "Generate Microsoft SQL Server T-SQL.");
        builder.AppendLine(
            "Use TOP instead of LIMIT.");
        builder.AppendLine();

        if (metadata.Tables.Count == 0)
        {
            builder.AppendLine(
                "No matching metadata was found.");
            builder.AppendLine();
            return builder.ToString();
        }

        builder.AppendLine("Relevant Tables");
        builder.AppendLine();

        foreach (var table in metadata.Tables
                     .OrderBy(t => t.SchemaName)
                     .ThenBy(t => t.DbTableName))
        {
            builder.AppendLine(
                $"- {table.SchemaName}.{table.DbTableName}");
        }

        builder.AppendLine();

        var groupedFields =
            metadata.Fields
                .GroupBy(f => f.DataTableId)
                .OrderBy(g => g.Key);

        foreach (var fieldGroup in groupedFields)
        {
            builder.AppendLine(
                $"Table: {fieldGroup.Key}");

            foreach (var field in fieldGroup
                         .OrderBy(f => f.FieldName))
            {
                builder.AppendLine(
                    $"  - {field.FieldName}");
            }

            builder.AppendLine();
        }

        if (metadata.Relationships.Count > 0)
        {
            builder.AppendLine("Relationships");
            builder.AppendLine();

            foreach (var relationship in metadata.Relationships
                         .OrderBy(r => r.RelationId))
            {
                builder.AppendLine(
                    $"- {relationship.RelationId}");
            }

            builder.AppendLine();
        }

        builder.AppendLine("SQL Rules");
        builder.AppendLine("- Use SQL Server syntax.");
        builder.AppendLine("- Use TOP instead of LIMIT.");
        builder.AppendLine("- Do not create tables.");
        builder.AppendLine("- Do not alter tables.");
        builder.AppendLine("- Do not invent column names.");
        builder.AppendLine("- Prefer metadata supplied above.");

        return builder.ToString();
    }
}