using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Represents an Azure DevOps project that contains the repository.
/// </summary>
public class Project
{
    /// <summary>
    /// Unique identifier for the project (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the project.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API URL to access the project via Azure DevOps REST API.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// State of the project.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}
