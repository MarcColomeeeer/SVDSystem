using System.Text.Json.Serialization;

namespace SVDSystem.Infrastructure.Dtos;

/// <summary>
/// A single vulnerability finding parsed from the Ollama model response.
/// </summary>
internal sealed class OllamaFinding
{
    /// <summary>
    /// One of: "None", "Low", "Medium", "High"
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = "None";

    /// <summary>
    /// Short classification of the vulnerability (e.g. "SQL Injection", "Hardcoded Secret").
    /// </summary>
    [JsonPropertyName("vulnerabilityType")]
    public string VulnerabilityType { get; set; } = string.Empty;

    /// <summary>
    /// Explanation of the vulnerability and suggested remediation.
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
}
