using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents a git commit reference in the pull request.
/// </summary>
public class Commit
{
    /// <summary>
    /// The full SHA-1 commit hash.
    /// </summary>
    [JsonPropertyName("commitId")]
    public string CommitId { get; set; } = string.Empty;

    /// <summary>
    /// API URL to access commit details via Azure DevOps REST API.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}