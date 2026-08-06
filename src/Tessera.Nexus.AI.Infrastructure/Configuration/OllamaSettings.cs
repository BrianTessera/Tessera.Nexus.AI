namespace Tessera.Nexus.AI.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for connecting to Ollama.
/// </summary>
public sealed class OllamaSettings
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// Ollama API base URL.
    /// </summary>
    public string BaseUrl { get; set; } =
        "http://localhost:11434";

    /// <summary>
    /// Default model used for SQL generation.
    /// </summary>
    public string Model { get; set; } =
        "qwen3.6:35b";

    /// <summary>
    /// HTTP timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Temperature used by the model.
    /// Lower values produce more deterministic SQL.
    /// </summary>
    public double Temperature { get; set; } = 0.1;

    /// <summary>
    /// Optional system prompt override.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Whether Ollama integration is enabled.
    /// Allows fallback to MockSqlGenerator during development.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
