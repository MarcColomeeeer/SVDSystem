using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents an Azure DevOps Pull Request Created webhook event.
/// </summary>
public class PullRequestCreatedEvent
{
    /// <summary>
    /// Unique identifier for this webhook event (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of event. Should be "git.pullrequest.created" for PR created events.
    /// </summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The publisher of the event. Typically "tfs" for Azure DevOps.
    /// </summary>
    [JsonPropertyName("publisherId")]
    public string PublisherId { get; set; } = string.Empty;

    /// <summary>
    /// The scope of the event.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Short message summarizing the event.
    /// </summary>
    [JsonPropertyName("message")]
    public WebhookMessage? Message { get; set; }

    /// <summary>
    /// Detailed message with more information about the event.
    /// </summary>
    [JsonPropertyName("detailedMessage")]
    public WebhookMessage? DetailedMessage { get; set; }

    /// <summary>
    /// The main payload containing pull request details.
    /// </summary>
    [JsonPropertyName("resource")]
    public PullRequestResource? Resource { get; set; }

    /// <summary>
    /// API version of the resource structure.
    /// </summary>
    [JsonPropertyName("resourceVersion")]
    public string ResourceVersion { get; set; } = string.Empty;

    /// <summary>
    /// Container information (collection, account, project).
    /// </summary>
    [JsonPropertyName("resourceContainers")]
    public ResourceContainers? ResourceContainers { get; set; }

    /// <summary>
    /// Timestamp when the webhook event was created.
    /// </summary>
    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }
}