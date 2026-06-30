using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Contains references to Azure DevOps organizational containers.
/// </summary>
public class ResourceContainers
{
    /// <summary>
    /// Collection-level container information.
    /// </summary>
    [JsonPropertyName("collection")]
    public ResourceContainer? Collection { get; set; }

    /// <summary>
    /// Account-level container information.
    /// </summary>
    [JsonPropertyName("account")]
    public ResourceContainer? Account { get; set; }

    /// <summary>
    /// Project-level container information.
    /// </summary>
    [JsonPropertyName("project")]
    public ResourceContainer? Project { get; set; }
}

/// <summary>
/// Represents a single Azure DevOps resource container.
/// </summary>
public class ResourceContainer
{
    /// <summary>
    /// Unique identifier for the container (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the container.
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;
}
