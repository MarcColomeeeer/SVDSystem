using System.Text.Json.Serialization;

namespace SVDSystem.Infrastructure.Dtos;

/// <summary>
/// A single message in an Ollama chat conversation.
/// </summary>
internal sealed class OllamaMessage
{
    /// <summary>
    /// The role of the message author: "system", "user", or "assistant".
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The text content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
