namespace SVDSystem.Infrastructure.Configuration;

public class OllamaSettings
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// Base URL of the local Ollama instance.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Timeout in seconds for each analysis request
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Ollama model to use (e.g. "llama3", "mistral", "codellama")
    /// </summary>
    public string Model { get; set; } = "mistral:7B";
}
