using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents a reviewer (user or group) assigned to a pull request.
/// </summary>
public class Reviewer
{
    /// <summary>
    /// URL specific to this reviewer's review.
    /// </summary>
    [JsonPropertyName("reviewerUrl")]
    public string? ReviewerUrl { get; set; }

    /// <summary>
    /// The reviewer's vote on the pull request.
    /// </summary>
    [JsonPropertyName("vote")]
    public int Vote { get; set; }

    /// <summary>
    /// Unique identifier for the reviewer (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the reviewer.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Unique name identifier for the reviewer.
    /// </summary>
    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; } = string.Empty;

    /// <summary>
    /// API URL to access reviewer identity details.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// URL to the reviewer's avatar image.
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this reviewer is a group/team rather than an individual user.
    /// </summary>
    [JsonPropertyName("isContainer")]
    public bool IsContainer { get; set; }
}
