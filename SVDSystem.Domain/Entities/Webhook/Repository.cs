using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents an Azure DevOps repository where the pull request was created.
/// </summary>
public class Repository
{
    /// <summary>
    /// Unique identifier for the repository (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the repository.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API URL to access the repository via Azure DevOps REST API.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Project information where this repository belongs.
    /// </summary>
    [JsonPropertyName("project")]
    public Project? Project { get; set; }

    /// <summary>
    /// Default branch of the repository.
    /// </summary>
    [JsonPropertyName("defaultBranch")]
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>
    /// Git clone URL for the repository.
    /// </summary>
    [JsonPropertyName("remoteUrl")]
    public string RemoteUrl { get; set; } = string.Empty;
}