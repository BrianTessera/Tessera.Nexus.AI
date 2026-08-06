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

    public async Task<IReadOnlyList<EpicorDataField>> GetRelevantFieldsAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return Array.Empty<EpicorDataField>();
        }

        var tables =
            await GetRelevantTablesAsync(
                userQuestion,
                cancellationToken);

        if (tables.Count == 0)
        {
            return Array.Empty<EpicorDataField>();
        }

        var allFields =
            new List<EpicorDataField>();

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

            allFields.AddRange(fields);
        }

        if (allFields.Count == 0)
        {
            return Array.Empty<EpicorDataField>();
        }

        var relevantFieldNames =
            GetLikelyFieldNames(userQuestion);

        var selectedFields =
            allFields
                .Where(field =>
                    IsCoreField(field.FieldName) ||
                    relevantFieldNames.Contains(field.FieldName))
                .GroupBy(field =>
                    new
                    {
                        field.DataTableId,
                        field.FieldName
                    })
                .Select(group => group.First())
                .OrderBy(field => field.DataTableId)
                .ThenBy(field => GetFieldRank(field.FieldName))
                .ThenBy(field => field.FieldName)
                .ToList();

        if (selectedFields.Count > 0)
        {
            return LimitFieldsPerTable(
                selectedFields,
                maxFieldsPerTable: 30);
        }

        return LimitFieldsPerTable(
            allFields,
            maxFieldsPerTable: 20);
    }

    public async Task<EpicorMetadataContext> GetMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        var tables =
            await GetRelevantTablesAsync(
                userQuestion,
                cancellationToken);

        var fields =
            await GetRelevantFieldsAsync(
                userQuestion,
                cancellationToken);

        var allRelationships =
            new List<EpicorRelation>();

        foreach (var table in tables)
        {
            if (string.IsNullOrWhiteSpace(table.SchemaName) ||
                string.IsNullOrWhiteSpace(table.DbTableName))
            {
                continue;
            }

            var relationships =
                await GetRelationshipsAsync(
                    table.SchemaName,
                    table.DbTableName,
                    cancellationToken);

            allRelationships.AddRange(relationships);
        }

        var distinctRelationships =
            allRelationships
                .GroupBy(relationship => relationship.RelationId)
                .Select(group => group.First())
                .OrderBy(relationship => relationship.RelationId)
                .Take(30)
                .ToList();

        return new EpicorMetadataContext
        {
            Tables = tables,
            Fields = fields,
            Relationships = distinctRelationships
        };
    }

    private static IReadOnlyList<EpicorDataField> LimitFieldsPerTable(
        IEnumerable<EpicorDataField> fields,
        int maxFieldsPerTable)
    {
        return fields
            .GroupBy(field => field.DataTableId)
            .SelectMany(group =>
                group
                    .OrderBy(field => GetFieldRank(field.FieldName))
                    .ThenBy(field => field.FieldName)
                    .Take(maxFieldsPerTable))
            .ToList();
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

    private static IReadOnlySet<string> GetLikelyFieldNames(
        string userQuestion)
    {
        var normalized =
            userQuestion.Trim().ToLowerInvariant();

        var fields =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        AddCommonFields(fields);

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
            AddFields(
                fields,
                "PackNum",
                "PackLine",
                "ShipDate",
                "CustNum",
                "ShipToNum",
                "ShipToCustNum",
                "ReadyToInvoice",
                "Invoiced",
                "EntryPerson",
                "TrackingNumber",
                "ShipViaCode",
                "ShipStatus",
                "Plant");
        }

        if (ContainsAny(
                normalized,
                "customer",
                "customers",
                "custnum",
                "custid",
                "cust id",
                "sold to"))
        {
            AddFields(
                fields,
                "CustNum",
                "CustID",
                "Name",
                "BTName",
                "ShipToNum",
                "City",
                "State",
                "Country",
                "TermsCode",
                "SalesRepCode");
        }

        if (ContainsAny(
                normalized,
                "value",
                "amount",
                "total",
                "revenue",
                "sales",
                "price",
                "extended",
                "dollars",
                "cost"))
        {
            AddFields(
                fields,
                "DocExtPrice",
                "ExtPrice",
                "DocInExtPrice",
                "InExtPrice",
                "DocUnitPrice",
                "UnitPrice",
                "OrderAmt",
                "DocOrderAmt",
                "TotalTax",
                "DocTotalTax",
                "TotalDiscount",
                "DocTotalDiscount",
                "SellingInventoryShipQty",
                "OurInventoryShipQty");
        }

        if (ContainsAny(
                normalized,
                "part",
                "parts",
                "partnum",
                "part number",
                "inventory",
                "item"))
        {
            AddFields(
                fields,
                "PartNum",
                "PartDescription",
                "LineDesc",
                "TypeCode",
                "IUM",
                "PUM",
                "ClassID",
                "WarehouseCode",
                "BinNum",
                "OnhandQty");
        }

        if (ContainsAny(
                normalized,
                "job",
                "jobs",
                "jobnum",
                "job number",
                "labor",
                "operation",
                "operations",
                "earned hours",
                "actual hours"))
        {
            AddFields(
                fields,
                "JobNum",
                "AssemblySeq",
                "OprSeq",
                "OpCode",
                "ActProdHours",
                "ProdStandard",
                "LaborQty",
                "LaborHrs",
                "PartNum",
                "ProdQty",
                "QtyCompleted",
                "JobClosed",
                "JobComplete");
        }

        if (ContainsAny(
                normalized,
                "order",
                "orders",
                "sales order",
                "release",
                "releases",
                "ordernum"))
        {
            AddFields(
                fields,
                "OrderNum",
                "OrderLine",
                "OrderRelNum",
                "OrderDate",
                "NeedByDate",
                "RequestDate",
                "OpenOrder",
                "OpenLine",
                "OpenRelease");
        }

        if (ContainsAny(
                normalized,
                "invoice",
                "invoices",
                "billing",
                "billed"))
        {
            AddFields(
                fields,
                "InvoiceNum",
                "InvoiceLine",
                "InvoiceDate",
                "DocInvoiceAmt",
                "InvoiceAmt",
                "PackNum",
                "PackLine");
        }

        return fields;
    }

    private static void AddCommonFields(
        HashSet<string> fields)
    {
        AddFields(
            fields,
            "Company",
            "SysRowID");
    }

    private static void AddFields(
        HashSet<string> fields,
        params string[] fieldNames)
    {
        foreach (var fieldName in fieldNames)
        {
            fields.Add(fieldName);
        }
    }

    private static bool IsCoreField(
        string fieldName)
    {
        return fieldName.Equals(
                   "Company",
                   StringComparison.OrdinalIgnoreCase)
               || fieldName.Equals(
                   "SysRowID",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static int GetFieldRank(
        string fieldName)
    {
        return fieldName.ToUpperInvariant() switch
        {
            "COMPANY" => 0,
            "PACKNUM" => 1,
            "SHIPDATE" => 2,
            "CUSTNUM" => 3,
            "CUSTID" => 4,
            "NAME" => 5,
            "PARTNUM" => 6,
            "ORDERNUM" => 7,
            "ORDERLINE" => 8,
            "ORDERRELNUM" => 9,
            "PACKLINE" => 10,
            "DOCEXTPRICE" => 11,
            "EXTPRICE" => 12,
            "DOCORDERAMT" => 13,
            "ORDERAMT" => 14,
            "JOBNUM" => 15,
            "ACTPRODHOURS" => 16,
            "PRODSTANDARD" => 17,
            "SYSROWID" => 99,
            _ => 50
        };
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