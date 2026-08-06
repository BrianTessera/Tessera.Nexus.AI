using System.Text;
using Tessera.Nexus.AI.Application.Contracts;

namespace Tessera.Nexus.AI.Application.Services;

public sealed class PromptBuilder : IPromptBuilder
{
    private const string DefaultSqlTemplateName = "SqlGeneration";

    private readonly IPromptTemplateRepository _promptTemplateRepository;
    private readonly IBusinessKnowledgeRepository _businessKnowledgeRepository;
    private readonly IBusinessRuleRepository _businessRuleRepository;
    private readonly IMetadataContextBuilder _metadataContextBuilder;

    public PromptBuilder(
        IPromptTemplateRepository promptTemplateRepository,
        IBusinessKnowledgeRepository businessKnowledgeRepository,
        IBusinessRuleRepository businessRuleRepository,
        IMetadataContextBuilder metadataContextBuilder)
    {
        _promptTemplateRepository = promptTemplateRepository;
        _businessKnowledgeRepository = businessKnowledgeRepository;
        _businessRuleRepository = businessRuleRepository;
        _metadataContextBuilder = metadataContextBuilder;
    }

    public async Task<string> BuildSqlPromptAsync(
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        return await BuildPromptAsync(
            DefaultSqlTemplateName,
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

        var metadataContext =
            await _metadataContextBuilder.BuildMetadataContextAsync(
                userQuestion,
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

            if (!string.IsNullOrWhiteSpace(rule.RuleCategory))
            {
                prompt.AppendLine(
                    $"Category: {rule.RuleCategory}");
            }

            if (!string.IsNullOrWhiteSpace(rule.RuleDescription))
            {
                prompt.AppendLine(
                    $"Description: {rule.RuleDescription}");
            }

            if (!string.IsNullOrWhiteSpace(rule.RuleLogicDescription))
            {
                prompt.AppendLine(
                    $"Logic: {rule.RuleLogicDescription}");
            }

            if (!string.IsNullOrWhiteSpace(rule.PromptInstruction))
            {
                prompt.AppendLine(
                    $"Instruction: {rule.PromptInstruction}");
            }

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

            if (!string.IsNullOrWhiteSpace(knowledge.PromptInstruction))
            {
                prompt.AppendLine(
                    $"Instruction: {knowledge.PromptInstruction}");
            }

            prompt.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(metadataContext))
        {
            prompt.AppendLine(metadataContext);
            prompt.AppendLine();
        }

        prompt.AppendLine("### USER REQUEST ###");
        prompt.AppendLine();
        prompt.AppendLine(userQuestion);
        prompt.AppendLine();

        prompt.AppendLine("### SQL RESPONSE RULES ###");
        prompt.AppendLine();
        prompt.AppendLine("- Return SQL only.");
        prompt.AppendLine("- Use Microsoft SQL Server T-SQL syntax.");
        prompt.AppendLine("- Use TOP instead of LIMIT.");
        prompt.AppendLine("- Use fully qualified Epicor table names when supplied.");
        prompt.AppendLine("- Do not invent table names.");
        prompt.AppendLine("- Do not invent column names.");
        prompt.AppendLine("- Do not include explanations.");
        prompt.AppendLine("- Do not include markdown code fences.");
        prompt.AppendLine();

        return prompt.ToString();
    }
}