using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Infrastructure.Configuration;

namespace Tessera.Nexus.AI.Infrastructure.AI;

/// <summary>
/// HTTP client for communicating with a local Ollama server.
/// </summary>
public sealed class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;

    public OllamaClient(
        HttpClient httpClient,
        IOptions<OllamaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _httpClient.BaseAddress =
                new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        }

        _httpClient.Timeout =
            TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));
        }

        if (string.IsNullOrWhiteSpace(_settings.Model))
        {
            throw new InvalidOperationException(
                "Ollama model is not configured.");
        }

        var request = new OllamaGenerateRequest
        {
            Model = _settings.Model,
            Prompt = prompt,
            Stream = false,
            Options = new OllamaGenerateOptions
            {
                Temperature = _settings.Temperature,
                NumPredict = _settings.MaxTokens
            }
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/generate",
                request,
                cancellationToken);

        var rawJson =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"""
                Ollama generation failed.

                Status: {(int)response.StatusCode} {response.ReasonPhrase}
                BaseAddress: {_httpClient.BaseAddress}
                Model: {_settings.Model}
                PromptLength: {prompt.Length}

                Response:
                {rawJson}
                """);
        }

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            throw new InvalidOperationException(
                $"""
                Ollama returned an empty HTTP response body.

                BaseAddress: {_httpClient.BaseAddress}
                Model: {_settings.Model}
                PromptLength: {prompt.Length}
                """);
        }

        OllamaGenerateResponse? result;

        try
        {
            result =
                JsonSerializer.Deserialize<OllamaGenerateResponse>(
                    rawJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"""
                Unable to parse Ollama generate response.

                BaseAddress: {_httpClient.BaseAddress}
                Model: {_settings.Model}
                PromptLength: {prompt.Length}

                Raw JSON:
                {rawJson}
                """,
                ex);
        }

        if (result is null)
        {
            throw new InvalidOperationException(
                $"""
                Ollama returned a response that deserialized to null.

                BaseAddress: {_httpClient.BaseAddress}
                Model: {_settings.Model}
                PromptLength: {prompt.Length}

                Raw JSON:
                {rawJson}
                """);
        }

        if (string.IsNullOrWhiteSpace(result.Response))
        {
            throw new InvalidOperationException(
                $"""
                Ollama returned an empty generated response.

                BaseAddress: {_httpClient.BaseAddress}
                ConfiguredModel: {_settings.Model}
                ResponseModel: {result.Model ?? "null"}
                Done: {result.Done}
                DoneReason: {result.DoneReason ?? "null"}

                PromptLength: {prompt.Length}
                PromptEvalCount: {result.PromptEvalCount?.ToString() ?? "null"}
                EvalCount: {result.EvalCount?.ToString() ?? "null"}

                TotalDuration: {result.TotalDuration?.ToString() ?? "null"}
                LoadDuration: {result.LoadDuration?.ToString() ?? "null"}
                PromptEvalDuration: {result.PromptEvalDuration?.ToString() ?? "null"}
                EvalDuration: {result.EvalDuration?.ToString() ?? "null"}

                Raw JSON:
                {rawJson}
                """);
        }

        return result.Response.Trim();
    }

    public async Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response =
                await _httpClient.GetAsync(
                    "api/tags",
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/tags",
                cancellationToken);

        var rawJson =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"""
                Unable to retrieve Ollama models.

                Status: {(int)response.StatusCode} {response.ReasonPhrase}
                BaseAddress: {_httpClient.BaseAddress}

                Response:
                {rawJson}
                """);
        }

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return Array.Empty<string>();
        }

        OllamaTagsResponse? result;

        try
        {
            result =
                JsonSerializer.Deserialize<OllamaTagsResponse>(
                    rawJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"""
                Unable to parse Ollama tags response.

                BaseAddress: {_httpClient.BaseAddress}

                Raw JSON:
                {rawJson}
                """,
                ex);
        }

        if (result?.Models is null)
        {
            return Array.Empty<string>();
        }

        return result.Models
            .Where(model => !string.IsNullOrWhiteSpace(model.Name))
            .Select(model => model.Name)
            .OrderBy(modelName => modelName)
            .ToList();
    }

    private sealed class OllamaGenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("options")]
        public OllamaGenerateOptions Options { get; set; } = new();
    }

    private sealed class OllamaGenerateOptions
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }
    }

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        [JsonPropertyName("total_duration")]
        public long? TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long? LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; set; }

        [JsonPropertyName("prompt_eval_duration")]
        public long? PromptEvalDuration { get; set; }

        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; set; }

        [JsonPropertyName("eval_duration")]
        public long? EvalDuration { get; set; }
    }

    private sealed class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModelInfo> Models { get; set; } = new();
    }

    private sealed class OllamaModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("modified_at")]
        public string? ModifiedAt { get; set; }

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("digest")]
        public string? Digest { get; set; }
    }
}