using System.Text.Json.Serialization;

namespace SVDSystem.Infrastructure.Dtos;

/// <summary>
/// Request payload for the Ollama <c>/api/chat</c> endpoint.
/// </summary>
internal sealed class OllamaChatRequest
{
    /// <summary>
    /// The Ollama model to use (e.g. "llama3", "codellama").
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The ordered list of messages that form the conversation context.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = [];

    /// <summary>
    /// When <see langword="false"/>, the full response is returned in one JSON object instead of a stream.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}
