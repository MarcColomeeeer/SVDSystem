using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents a message in different formats from the webhook event.
/// </summary>
public class WebhookMessage
{
    /// <summary>
    /// Plain text version of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// HTML version of the message.
    /// </summary>
    [JsonPropertyName("html")]
    public string Html { get; set; } = string.Empty;

    /// <summary>
    /// Markdown version of the message.
    /// </summary>
    [JsonPropertyName("markdown")]
    public string Markdown { get; set; } = string.Empty;
}
