using System.Text;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Application.Services;

/// <summary>
/// Builds Epicor metadata context for prompt grounding.
///
/// This first version uses curated Epicor table guidance for common domains.
/// Later versions can replace or extend this with dynamic metadata lookup
/// from IMetadataRepository.
/// </summary>
public sealed class MetadataContextBuilder : IMetadataContextBuilder
{
    public Task<string> BuildMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return Task.FromResult(string.Empty);
        }

        var normalizedQuestion =
            userQuestion.Trim().ToLowerInvariant();

        var context = new StringBuilder();

        context.AppendLine("### EPICOR METADATA CONTEXT ###");
        context.AppendLine();
        context.AppendLine("Use only the supplied Epicor metadata context when generating SQL.");
        context.AppendLine("Do not invent table names, column names, or relationships.");
        context.AppendLine("Generate Microsoft SQL Server T-SQL, not MySQL, PostgreSQL, or SQLite syntax.");
        context.AppendLine("Use TOP instead of LIMIT.");
        context.AppendLine();

        var hasAnyDomain =
            AppendShipmentContextIfRelevant(
                normalizedQuestion,
                context);

        hasAnyDomain =
            AppendPartContextIfRelevant(
                normalizedQuestion,
                context) || hasAnyDomain;

        hasAnyDomain =
            AppendJobContextIfRelevant(
                normalizedQuestion,
                context) || hasAnyDomain;

        hasAnyDomain =
            AppendCustomerContextIfRelevant(
                normalizedQuestion,
                context) || hasAnyDomain;

        if (!hasAnyDomain)
        {
            AppendGeneralEpicorContext(context);
        }

        return Task.FromResult(context.ToString());
    }

    private static bool AppendShipmentContextIfRelevant(
        string question,
        StringBuilder context)
    {
        if (!ContainsAny(
                question,
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
            return false;
        }

        context.AppendLine("Domain: Shipments");
        context.AppendLine();
        context.AppendLine("Primary table:");
        context.AppendLine("- Erp.ShipHead");
        context.AppendLine();
        context.AppendLine("Relevant columns:");
        context.AppendLine("- Company");
        context.AppendLine("- PackNum");
        context.AppendLine("- ShipDate");
        context.AppendLine("- CustNum");
        context.AppendLine("- ShipToNum");
        context.AppendLine("- ReadyToInvoice");
        context.AppendLine("- Invoiced");
        context.AppendLine("- EntryPerson");
        context.AppendLine();
        context.AppendLine("Detail table:");
        context.AppendLine("- Erp.ShipDtl");
        context.AppendLine();
        context.AppendLine("Relevant detail columns:");
        context.AppendLine("- Company");
        context.AppendLine("- PackNum");
        context.AppendLine("- PackLine");
        context.AppendLine("- OrderNum");
        context.AppendLine("- OrderLine");
        context.AppendLine("- OrderRelNum");
        context.AppendLine("- PartNum");
        context.AppendLine("- LineDesc");
        context.AppendLine("- SellingInventoryShipQty");
        context.AppendLine("- InventoryShipUOM");
        context.AppendLine();
        context.AppendLine("Relationship:");
        context.AppendLine("- Erp.ShipHead.Company = Erp.ShipDtl.Company");
        context.AppendLine("- Erp.ShipHead.PackNum = Erp.ShipDtl.PackNum");
        context.AppendLine();
        context.AppendLine("Example pattern:");
        context.AppendLine("SELECT TOP (5)");
        context.AppendLine("    PackNum,");
        context.AppendLine("    ShipDate");
        context.AppendLine("FROM Erp.ShipHead");
        context.AppendLine("ORDER BY ShipDate DESC, PackNum DESC;");
        context.AppendLine();

        return true;
    }

    private static bool AppendPartContextIfRelevant(
        string question,
        StringBuilder context)
    {
        if (!ContainsAny(
                question,
                "part",
                "parts",
                "partnum",
                "part number",
                "item",
                "items",
                "inventory"))
        {
            return false;
        }

        context.AppendLine("Domain: Parts and Inventory");
        context.AppendLine();
        context.AppendLine("Primary table:");
        context.AppendLine("- Erp.Part");
        context.AppendLine();
        context.AppendLine("Relevant columns:");
        context.AppendLine("- Company");
        context.AppendLine("- PartNum");
        context.AppendLine("- PartDescription");
        context.AppendLine("- TypeCode");
        context.AppendLine("- IUM");
        context.AppendLine("- PUM");
        context.AppendLine("- ClassID");
        context.AppendLine("- InActive");
        context.AppendLine();
        context.AppendLine("Inventory quantity table:");
        context.AppendLine("- Erp.PartBin");
        context.AppendLine();
        context.AppendLine("Relevant PartBin columns:");
        context.AppendLine("- Company");
        context.AppendLine("- PartNum");
        context.AppendLine("- WarehouseCode");
        context.AppendLine("- BinNum");
        context.AppendLine("- OnhandQty");
        context.AppendLine();
        context.AppendLine("Relationship:");
        context.AppendLine("- Erp.Part.Company = Erp.PartBin.Company");
        context.AppendLine("- Erp.Part.PartNum = Erp.PartBin.PartNum");
        context.AppendLine();

        return true;
    }

    private static bool AppendJobContextIfRelevant(
        string question,
        StringBuilder context)
    {
        if (!ContainsAny(
                question,
                "job",
                "jobs",
                "jobnum",
                "job number",
                "labor",
                "operation",
                "operations",
                "manufacturing",
                "production"))
        {
            return false;
        }

        context.AppendLine("Domain: Jobs and Production");
        context.AppendLine();
        context.AppendLine("Primary table:");
        context.AppendLine("- Erp.JobHead");
        context.AppendLine();
        context.AppendLine("Relevant columns:");
        context.AppendLine("- Company");
        context.AppendLine("- JobNum");
        context.AppendLine("- PartNum");
        context.AppendLine("- PartDescription");
        context.AppendLine("- ProdQty");
        context.AppendLine("- QtyCompleted");
        context.AppendLine("- JobClosed");
        context.AppendLine("- JobComplete");
        context.AppendLine("- DueDate");
        context.AppendLine();
        context.AppendLine("Operation table:");
        context.AppendLine("- Erp.JobOper");
        context.AppendLine();
        context.AppendLine("Relevant JobOper columns:");
        context.AppendLine("- Company");
        context.AppendLine("- JobNum");
        context.AppendLine("- AssemblySeq");
        context.AppendLine("- OprSeq");
        context.AppendLine("- OpCode");
        context.AppendLine("- ProdStandard");
        context.AppendLine("- ActProdHours");
        context.AppendLine();
        context.AppendLine("Relationship:");
        context.AppendLine("- Erp.JobHead.Company = Erp.JobOper.Company");
        context.AppendLine("- Erp.JobHead.JobNum = Erp.JobOper.JobNum");
        context.AppendLine();

        return true;
    }

    private static bool AppendCustomerContextIfRelevant(
        string question,
        StringBuilder context)
    {
        if (!ContainsAny(
                question,
                "customer",
                "customers",
                "custnum",
                "cust number",
                "sold to",
                "ship to"))
        {
            return false;
        }

        context.AppendLine("Domain: Customers");
        context.AppendLine();
        context.AppendLine("Primary table:");
        context.AppendLine("- Erp.Customer");
        context.AppendLine();
        context.AppendLine("Relevant columns:");
        context.AppendLine("- Company");
        context.AppendLine("- CustNum");
        context.AppendLine("- CustID");
        context.AppendLine("- Name");
        context.AppendLine("- City");
        context.AppendLine("- State");
        context.AppendLine("- Country");
        context.AppendLine("- TermsCode");
        context.AppendLine();
        context.AppendLine("Common shipment relationship:");
        context.AppendLine("- Erp.Customer.Company = Erp.ShipHead.Company");
        context.AppendLine("- Erp.Customer.CustNum = Erp.ShipHead.CustNum");
        context.AppendLine();

        return true;
    }

    private static void AppendGeneralEpicorContext(
        StringBuilder context)
    {
        context.AppendLine("General Epicor guidance:");
        context.AppendLine("- Prefer fully qualified table names such as Erp.Part, Erp.JobHead, Erp.ShipHead.");
        context.AppendLine("- Include Company filters when available.");
        context.AppendLine("- Use SQL Server syntax.");
        context.AppendLine("- Use TOP (n) instead of LIMIT.");
        context.AppendLine("- Do not use generic table names such as shipments, parts, jobs, or customers unless explicitly present in metadata.");
        context.AppendLine();
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