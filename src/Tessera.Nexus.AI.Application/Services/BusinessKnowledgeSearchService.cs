using System.Text;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class BusinessKnowledgeSearchService : IBusinessKnowledgeSearchService
{
    private readonly IBusinessKnowledgeRepository _businessKnowledgeRepository;

    public BusinessKnowledgeSearchService(
        IBusinessKnowledgeRepository businessKnowledgeRepository)
    {
        _businessKnowledgeRepository = businessKnowledgeRepository;
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> FindRelevantKnowledgeAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        var normalizedMaxResults =
            NormalizeMaxResults(maxResults);

        var results =
            await _businessKnowledgeRepository.GetRelevantKnowledgeAsync(
                userQuestion.Trim(),
                semanticDomain,
                company,
                plant,
                normalizedMaxResults,
                cancellationToken);

        return results
            .Where(IsUsableKnowledge)
            .OrderByDescending(knowledge => knowledge.Priority)
            .ThenBy(knowledge => knowledge.KnowledgeType)
            .ThenBy(knowledge => knowledge.KnowledgeTitle)
            .Take(normalizedMaxResults)
            .ToList();
    }

    public async Task<string> BuildKnowledgeContextAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var relevantKnowledge =
            await FindRelevantKnowledgeAsync(
                userQuestion,
                semanticDomain,
                company,
                plant,
                maxResults,
                cancellationToken);

        var globalInstructions =
            await GetGlobalPromptInstructionsAsync(
                cancellationToken);

        var combinedKnowledge =
            MergeKnowledge(
                    globalInstructions,
                    relevantKnowledge)
                .OrderByDescending(knowledge => knowledge.Priority)
                .ThenBy(knowledge => knowledge.KnowledgeType)
                .ThenBy(knowledge => knowledge.KnowledgeTitle)
                .Take(NormalizeMaxResults(maxResults) + 10)
                .ToList();

        if (combinedKnowledge.Count == 0)
        {
            return string.Empty;
        }

        var builder =
            new StringBuilder();

        builder.AppendLine("### BUSINESS KNOWLEDGE CONTEXT ###");
        builder.AppendLine();

        builder.AppendLine(
            "Use the following Tessera business knowledge when interpreting the user request and generating SQL.");

        builder.AppendLine(
            "Do not invent business rules, definitions, aliases, customer mappings, metrics, joins, or operational meanings.");

        builder.AppendLine(
            "If business knowledge conflicts with generic assumptions, prefer the supplied Tessera business knowledge.");

        builder.AppendLine();

        foreach (var knowledge in combinedKnowledge)
        {
            AppendKnowledgeEntry(
                builder,
                knowledge);
        }

        builder.AppendLine("Business Knowledge Rules");
        builder.AppendLine("- Use active and applicable business knowledge only.");
        builder.AppendLine("- Prefer higher-priority knowledge when multiple entries apply.");
        builder.AppendLine("- Use PromptInstruction values as direct instructions to the model.");
        builder.AppendLine("- Treat customer aliases, acronyms, and business-specific terms as authoritative when supplied.");
        builder.AppendLine("- Do not use business knowledge to perform unsafe SQL operations.");
        builder.AppendLine();

        return builder.ToString();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> FindByTermAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        var results =
            await _businessKnowledgeRepository.SearchAsync(
                term.Trim(),
                maxResults: 50,
                cancellationToken);

        return results
            .Where(IsUsableKnowledge)
            .Where(knowledge =>
                MatchesTerm(
                    knowledge,
                    term))
            .OrderByDescending(knowledge => knowledge.Priority)
            .ThenBy(knowledge => knowledge.KnowledgeType)
            .ThenBy(knowledge => knowledge.KnowledgeTitle)
            .ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> FindByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(semanticDomain))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        var results =
            await _businessKnowledgeRepository.GetBySemanticDomainAsync(
                semanticDomain.Trim(),
                cancellationToken);

        return results
            .Where(IsUsableKnowledge)
            .OrderByDescending(knowledge => knowledge.Priority)
            .ThenBy(knowledge => knowledge.KnowledgeType)
            .ThenBy(knowledge => knowledge.KnowledgeTitle)
            .ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> FindByObjectAsync(
        string objectType,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectType) ||
            string.IsNullOrWhiteSpace(objectName))
        {
            return Array.Empty<BusinessKnowledge>();
        }

        var results =
            await _businessKnowledgeRepository.SearchAsync(
                objectName.Trim(),
                maxResults: 100,
                cancellationToken);

        return results
            .Where(IsUsableKnowledge)
            .Where(knowledge =>
                MatchesObject(
                    knowledge,
                    objectType,
                    objectName))
            .OrderByDescending(knowledge => knowledge.Priority)
            .ThenBy(knowledge => knowledge.KnowledgeType)
            .ThenBy(knowledge => knowledge.KnowledgeTitle)
            .ToList();
    }

    public async Task<IReadOnlyList<BusinessKnowledge>> GetGlobalPromptInstructionsAsync(
        CancellationToken cancellationToken = default)
    {
        var results =
            await _businessKnowledgeRepository.GetActiveAsync(
                cancellationToken);

        return results
            .Where(IsUsableKnowledge)
            .Where(IsGlobalPromptInstruction)
            .OrderByDescending(knowledge => knowledge.Priority)
            .ThenBy(knowledge => knowledge.KnowledgeType)
            .ThenBy(knowledge => knowledge.KnowledgeTitle)
            .Take(25)
            .ToList();
    }

    private static void AppendKnowledgeEntry(
        StringBuilder builder,
        BusinessKnowledge knowledge)
    {
        builder.AppendLine(
            $"- Title: {knowledge.KnowledgeTitle}");

        builder.AppendLine(
            $"  Type: {knowledge.KnowledgeType}");

        if (!string.IsNullOrWhiteSpace(
                knowledge.SemanticDomain))
        {
            builder.AppendLine(
                $"  Semantic Domain: {knowledge.SemanticDomain}");
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.Company))
        {
            builder.AppendLine(
                $"  Company: {knowledge.Company}");
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.Plant))
        {
            builder.AppendLine(
                $"  Plant: {knowledge.Plant}");
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToTerm))
        {
            builder.AppendLine(
                $"  Applies To Term: {knowledge.AppliesToTerm}");
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToObjectType))
        {
            builder.AppendLine(
                $"  Applies To Object Type: {knowledge.AppliesToObjectType}");
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToObjectName))
        {
            builder.AppendLine(
                $"  Applies To Object Name: {knowledge.AppliesToObjectName}");
        }

        builder.AppendLine(
            $"  Knowledge: {NormalizeMultilineText(knowledge.KnowledgeText)}");

        if (!string.IsNullOrWhiteSpace(
                knowledge.PromptInstruction))
        {
            builder.AppendLine(
                $"  Prompt Instruction: {NormalizeMultilineText(knowledge.PromptInstruction)}");
        }

        builder.AppendLine(
            $"  Priority: {knowledge.Priority}");

        builder.AppendLine();
    }

    private static IReadOnlyList<BusinessKnowledge> MergeKnowledge(
        IReadOnlyList<BusinessKnowledge> first,
        IReadOnlyList<BusinessKnowledge> second)
    {
        var results =
            new List<BusinessKnowledge>();

        var seenIds =
            new HashSet<int>();

        foreach (var item in first)
        {
            if (seenIds.Add(
                    item.BusinessKnowledgeId))
            {
                results.Add(
                    item);
            }
        }

        foreach (var item in second)
        {
            if (seenIds.Add(
                    item.BusinessKnowledgeId))
            {
                results.Add(
                    item);
            }
        }

        return results;
    }

    private static bool IsUsableKnowledge(
        BusinessKnowledge knowledge)
    {
        return knowledge.IsActive &&
               !string.IsNullOrWhiteSpace(knowledge.KnowledgeTitle) &&
               !string.IsNullOrWhiteSpace(knowledge.KnowledgeText);
    }

    private static bool IsGlobalPromptInstruction(
        BusinessKnowledge knowledge)
    {
        if (string.IsNullOrWhiteSpace(
                knowledge.PromptInstruction))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToTerm))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToObjectType))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                knowledge.AppliesToObjectName))
        {
            return false;
        }

        return string.Equals(
                   knowledge.KnowledgeType,
                   "PromptInstruction",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   knowledge.KnowledgeType,
                   "GlobalInstruction",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   knowledge.KnowledgeType,
                   "SemanticRule",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesTerm(
        BusinessKnowledge knowledge,
        string term)
    {
        var normalizedTerm =
            term.Trim();

        return Contains(
                   knowledge.AppliesToTerm,
                   normalizedTerm)
               || Contains(
                   knowledge.KnowledgeTitle,
                   normalizedTerm)
               || Contains(
                   knowledge.KnowledgeText,
                   normalizedTerm)
               || Contains(
                   knowledge.PromptInstruction,
                   normalizedTerm);
    }

    private static bool MatchesObject(
        BusinessKnowledge knowledge,
        string objectType,
        string objectName)
    {
        var objectTypeMatches =
            Contains(
                knowledge.AppliesToObjectType,
                objectType);

        var objectNameMatches =
            Contains(
                knowledge.AppliesToObjectName,
                objectName)
            || Contains(
                knowledge.AppliesToTerm,
                objectName)
            || Contains(
                knowledge.KnowledgeTitle,
                objectName)
            || Contains(
                knowledge.KnowledgeText,
                objectName);

        return objectTypeMatches &&
               objectNameMatches;
    }

    private static bool Contains(
        string? source,
        string value)
    {
        if (string.IsNullOrWhiteSpace(source) ||
            string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return source.Contains(
            value,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeMultilineText(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Trim();
    }

    private static int NormalizeMaxResults(
        int maxResults)
    {
        if (maxResults <= 0)
        {
            return 10;
        }

        return maxResults > 50
            ? 50
            : maxResults;
    }
}