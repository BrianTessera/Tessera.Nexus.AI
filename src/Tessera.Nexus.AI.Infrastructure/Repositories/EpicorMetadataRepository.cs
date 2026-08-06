using Dapper;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Infrastructure.Repositories;

public sealed class EpicorMetadataRepository : IEpicorMetadataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EpicorMetadataRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<EpicorDataTable>> SearchTablesAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<EpicorDataTable>();
        }

        const string sql = """
            SELECT TOP (50)
                CAST(0 AS bigint) AS EpicorDataTableId,
                CONCAT(s.name, '.', t.name) AS DataTableId,
                s.name AS SchemaName,
                t.name AS DbTableName
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            WHERE
                s.name LIKE @Search
                OR t.name LIKE @Search
                OR CONCAT(s.name, '.', t.name) LIKE @Search
            ORDER BY
                s.name,
                t.name;
            """;

        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    Search = $"%{searchText.Trim()}%"
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<EpicorDataTable>(
                command);

        return results.ToList();
    }

    public async Task<EpicorDataTable?> GetTableAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (1)
                CAST(0 AS bigint) AS EpicorDataTableId,
                CONCAT(s.name, '.', t.name) AS DataTableId,
                s.name AS SchemaName,
                t.name AS DbTableName
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            WHERE
                s.name = @SchemaName
                AND t.name = @TableName;
            """;

        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    SchemaName = schemaName,
                    TableName = tableName
                },
                cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<EpicorDataTable>(
            command);
    }

    public async Task<IReadOnlyList<EpicorDataField>> GetFieldsAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                CAST(0 AS bigint) AS EpicorDataFieldId,
                CONCAT(s.name, '.', t.name) AS DataTableId,
                c.name AS FieldName
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            INNER JOIN sys.columns c
                ON t.object_id = c.object_id
            WHERE
                s.name = @SchemaName
                AND t.name = @TableName
            ORDER BY
                c.column_id;
            """;

        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    SchemaName = schemaName,
                    TableName = tableName
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<EpicorDataField>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<EpicorRelation>> GetRelationshipsAsync(
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                CAST(0 AS bigint) AS EpicorRelationId,
                CONCAT(
                    fk.name,
                    ': ',
                    parentSchema.name,
                    '.',
                    parentTable.name,
                    '.',
                    parentColumn.name,
                    ' -> ',
                    referencedSchema.name,
                    '.',
                    referencedTable.name,
                    '.',
                    referencedColumn.name
                ) AS RelationId
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc
                ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.tables parentTable
                ON fkc.parent_object_id = parentTable.object_id
            INNER JOIN sys.schemas parentSchema
                ON parentTable.schema_id = parentSchema.schema_id
            INNER JOIN sys.columns parentColumn
                ON fkc.parent_object_id = parentColumn.object_id
                AND fkc.parent_column_id = parentColumn.column_id
            INNER JOIN sys.tables referencedTable
                ON fkc.referenced_object_id = referencedTable.object_id
            INNER JOIN sys.schemas referencedSchema
                ON referencedTable.schema_id = referencedSchema.schema_id
            INNER JOIN sys.columns referencedColumn
                ON fkc.referenced_object_id = referencedColumn.object_id
                AND fkc.referenced_column_id = referencedColumn.column_id
            WHERE
                (
                    parentSchema.name = @SchemaName
                    AND parentTable.name = @TableName
                )
                OR
                (
                    referencedSchema.name = @SchemaName
                    AND referencedTable.name = @TableName
                )
            ORDER BY
                fk.name;
            """;

        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    SchemaName = schemaName,
                    TableName = tableName
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<EpicorRelation>(
                command);

        return results.ToList();
    }

    public async Task<IReadOnlyList<EpicorDataTable>> GetRelevantTablesAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return Array.Empty<EpicorDataTable>();
        }

        var tableNames =
            GetLikelyTableNames(userQuestion);

        if (tableNames.Count == 0)
        {
            return await SearchTablesAsync(
                userQuestion,
                cancellationToken);
        }

        const string sql = """
            SELECT
                CAST(0 AS bigint) AS EpicorDataTableId,
                CONCAT(s.name, '.', t.name) AS DataTableId,
                s.name AS SchemaName,
                t.name AS DbTableName
            FROM sys.tables t
            INNER JOIN sys.schemas s
                ON t.schema_id = s.schema_id
            WHERE
                s.name = 'Erp'
                AND t.name IN @TableNames
            ORDER BY
                t.name;
            """;

        using var connection =
            _connectionFactory.CreateEpicorConnection();

        var command =
            new CommandDefinition(
                sql,
                new
                {
                    TableNames = tableNames
                },
                cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<EpicorDataTable>(
                command);

        return results.ToList();
    }

    public async Task<EpicorMetadataContext> GetMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        var tables =
            await GetRelevantTablesAsync(
                userQuestion,
                cancellationToken);

        var allFields =
            new List<EpicorDataField>();

        var allRelationships =
            new List<EpicorRelation>();

        foreach (var table in tables)
        {
            if (string.IsNullOrWhiteSpace(table.SchemaName) ||
                string.IsNullOrWhiteSpace(table.DbTableName))
            {
                continue;
            }

            var fields =
                await GetFieldsAsync(
                    table.SchemaName,
                    table.DbTableName,
                    cancellationToken);

            var relationships =
                await GetRelationshipsAsync(
                    table.SchemaName,
                    table.DbTableName,
                    cancellationToken);

            allFields.AddRange(fields);
            allRelationships.AddRange(relationships);
        }

        return new EpicorMetadataContext
        {
            Tables = tables,
            Fields = allFields,
            Relationships = allRelationships
        };
    }

    private static IReadOnlyList<string> GetLikelyTableNames(
        string userQuestion)
    {
        var normalized =
            userQuestion.Trim().ToLowerInvariant();

        var tables =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        if (ContainsAny(
                normalized,
                "shipment",
                "shipments",
                "ship",
                "shipped",
                "pack",
                "packnum",
                "pack number",
                "shipment number",
                "shipment numbers",
                "packing slip",
                "pack slip"))
        {
            tables.Add("ShipHead");
            tables.Add("ShipDtl");
        }

        if (ContainsAny(
                normalized,
                "part",
                "parts",
                "partnum",
                "part number",
                "inventory",
                "on hand",
                "onhand",
                "warehouse",
                "bin"))
        {
            tables.Add("Part");
            tables.Add("PartBin");
        }

        if (ContainsAny(
                normalized,
                "job",
                "jobs",
                "jobnum",
                "job number",
                "production",
                "labor",
                "operation",
                "operations",
                "earned hours",
                "actual hours"))
        {
            tables.Add("JobHead");
            tables.Add("JobOper");
        }

        if (ContainsAny(
                normalized,
                "customer",
                "customers",
                "custnum",
                "custid",
                "cust id",
                "sold to",
                "ship to"))
        {
            tables.Add("Customer");
            tables.Add("ShipTo");
        }

        if (ContainsAny(
                normalized,
                "sales order",
                "order",
                "orders",
                "order number",
                "ordernum",
                "release",
                "releases"))
        {
            tables.Add("OrderHed");
            tables.Add("OrderDtl");
            tables.Add("OrderRel");
        }

        if (ContainsAny(
                normalized,
                "invoice",
                "invoices",
                "ar invoice",
                "billing"))
        {
            tables.Add("InvcHead");
            tables.Add("InvcDtl");
        }

        if (ContainsAny(
                normalized,
                "purchase order",
                "po",
                "supplier",
                "vendor"))
        {
            tables.Add("POHeader");
            tables.Add("PODetail");
            tables.Add("Vendor");
        }

        return tables.ToList();
    }

    private static bool ContainsAny(
        string source,
        params string[] values)
    {
        foreach (var value in values)
        {
            if (source.Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}