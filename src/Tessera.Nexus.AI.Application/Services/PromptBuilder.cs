using System.Text;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class PromptBuilder : IPromptBuilder
{
    private readonly IPromptTemplateRepository _promptTemplateRepository;
    private readonly IBusinessKnowledgeRepository _businessKnowledgeRepository;
    private readonly IBusinessRuleRepository _businessRuleRepository;

    public PromptBuilder(
        IPromptTemplateRepository promptTemplateRepository,
        IBusinessKnowledgeRepository businessKnowledgeRepository,
        IBusinessRuleRepository businessRuleRepository)
    {
        _promptTemplateRepository = promptTemplateRepository;
        _businessKnowledgeRepository = businessKnowledgeRepository;
        _businessRuleRepository = businessRuleRepository;
    }

    public async Task<string> BuildSqlPromptAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        return await BuildPromptAsync(
            "SQL Generation",
            userQuestion,
            cancellationToken);
    }

    public async Task<string> BuildPromptAsync(
        string templateName,
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        var template =
            await _promptTemplateRepository.GetActiveTemplateAsync(
                templateName,
                cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException(
                $"Prompt template '{templateName}' was not found.");
        }

        var businessRules =
            await _businessRuleRepository.GetActiveAsync(
                cancellationToken);

        var businessKnowledge =
            await _businessKnowledgeRepository.GetActiveAsync(
                cancellationToken);

        var prompt = new StringBuilder();

        prompt.AppendLine("### PROMPT TEMPLATE ###");
        prompt.AppendLine();
        prompt.AppendLine(template.TemplateText);
        prompt.AppendLine();

        prompt.AppendLine("### BUSINESS RULES ###");
        prompt.AppendLine();

        foreach (var rule in businessRules
                     .OrderBy(r => r.RuleName))
        {
            prompt.AppendLine(
                $"Rule: {rule.RuleName}");

            prompt.AppendLine(
                $"Type: {rule.RuleType}");

            prompt.AppendLine();
        }

        prompt.AppendLine("### BUSINESS KNOWLEDGE ###");
        prompt.AppendLine();

        foreach (var knowledge in businessKnowledge
                     .OrderBy(k => k.KnowledgeTitle))
        {
            prompt.AppendLine(
                $"Title: {knowledge.KnowledgeTitle}");

            prompt.AppendLine(
                knowledge.KnowledgeText);

            prompt.AppendLine();
        }

        prompt.AppendLine("### USER REQUEST ###");
        prompt.AppendLine();
        prompt.AppendLine(userQuestion);
        prompt.AppendLine();

        return prompt.ToString();
    }
}