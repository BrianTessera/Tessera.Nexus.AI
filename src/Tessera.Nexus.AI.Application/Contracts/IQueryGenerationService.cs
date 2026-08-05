using Tessera.Nexus.AI.Application.DTOs;

namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Orchestrates the AI query generation workflow.
///
/// User Question
///     ↓
/// PromptBuilder
///     ↓
/// SqlGenerator
///     ↓
/// QueryResponse
/// </summary>
public interface IQueryGenerationService
{
    /// <summary>
    /// Generates SQL from a natural language request.
    /// </summary>
    Task<QueryResponse> GenerateQueryAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default);
}