using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IPromptTemplateRepository
{
    Task<PromptTemplate?> GetByIdAsync(
        int promptTemplateId,
        CancellationToken cancellationToken = default);

    Task<PromptTemplate?> GetActiveTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptTemplate>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptTemplate>> GetByTypeAsync(
        string templateType,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        PromptTemplate template,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        PromptTemplate template,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int promptTemplateId,
        string modifiedBy,
        CancellationToken cancellationToken = default);
}