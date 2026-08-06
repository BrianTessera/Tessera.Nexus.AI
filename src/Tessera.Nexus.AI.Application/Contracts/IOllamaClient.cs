namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Low-level client responsible for communicating
/// with an Ollama server.
/// </summary>
public interface IOllamaClient
{
    /// <summary>
    /// Sends a prompt to the configured Ollama model
    /// and returns the generated response text.
    /// </summary>
    Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies connectivity to the Ollama server.
    /// </summary>
    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves available models from the Ollama server.
    /// </summary>
    Task<IReadOnlyList<string>> GetModelsAsync(
        CancellationToken cancellationToken = default);
}
