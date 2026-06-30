using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents an Azure DevOps user or group identity.
/// </summary>
public class Identity
{
    /// <summary>
    /// Unique identifier for the identity (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the user or group.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Unique name identifier.
    /// </summary>
    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; } = string.Empty;

    /// <summary>
    /// API URL to access identity details.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// URL to the user's or group's avatar image.
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
}
