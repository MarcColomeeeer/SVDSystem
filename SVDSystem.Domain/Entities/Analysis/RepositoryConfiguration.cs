using SVDSystem.Domain.Entities.Vulnerability;

namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// Per-repository processing configuration managed through the frontend.
/// </summary>
public class RepositoryConfiguration
{
    /// <summary>
    /// Internal primary key.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Azure DevOps repository GUID (Repository.Id in the webhook payload).
    /// </summary>
    public string RepositoryId { get; set; } = string.Empty;

    /// <summary>
    /// Repository name.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Azure DevOps project name.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Remote URL of the repository (used for cloning).
    /// </summary>
    public string RemoteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether this repository's PRs should be processed at all.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// When true, the system prompt will focus on <see cref="VulnerabilityCategories"/>.
    /// When false, the system prompt will analyze for any type of vulnerability.
    /// </summary>
    public bool UseCategories { get; set; } = false;

    /// <summary>
    /// Additional custom instructions appended to the system prompt.
    /// </summary>
    public string? CustomPrompt { get; set; }

    /// <summary>
    /// Minimum vulnerability level that should be reported. Cannot be None.
    /// </summary>
    public VulnerabilityLevel SeverityThreshold { get; set; } = VulnerabilityLevel.Low;

    /// <summary>
    /// Comma-separated list of vulnerability categories to report.
    /// </summary>
    public string VulnerabilityCategories { get; set; } = string.Empty;

    /// <summary>
    /// File-path prefixes / patterns to skip during diff analysis.
    /// </summary>
    public string IgnorePaths { get; set; } = string.Empty;

    /// <summary>
    /// File-extension whitelist for analysis (e.g. ".cs,.sql,.py").
    /// When empty, all file types are analyzed.
    /// </summary>
    public string FileTypeFilters { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include newly added files in the analysis.
    /// </summary>
    public bool IncludeAddedFiles { get; set; } = true;

    /// <summary>
    /// Whether to include deleted files in the analysis.
    /// </summary>
    public bool IncludeDeletedFiles { get; set; } = true;

    /// <summary>
    /// Whether to include modified files in the analysis.
    /// </summary>
    public bool IncludeModifiedFiles { get; set; } = true;

    // ── Navigation ───────────────────────────────────────────────────────────

    /// <summary>
    /// List of users with access to this repository's configuration (many-to-many relationship).
    /// </summary>
    public ICollection<UserRepositoryAccess> UserAccess { get; set; } = [];

    private static readonly char[] Delimiter = [','];

    /// <summary>
    /// Parsed list of vulnerability categories.
    /// </summary>
    public IReadOnlyList<string> GetVulnerabilityCategories() =>
        string.IsNullOrWhiteSpace(VulnerabilityCategories)
            ? []
            : VulnerabilityCategories.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Parsed list of ignore-path prefixes.
    /// </summary>
    public IReadOnlyList<string> GetIgnorePaths() =>
        string.IsNullOrWhiteSpace(IgnorePaths)
            ? []
            : IgnorePaths.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Parsed list of file-extension filters.
    /// </summary>
    public IReadOnlyList<string> GetFileTypeFilters() =>
        string.IsNullOrWhiteSpace(FileTypeFilters)
            ? []
            : FileTypeFilters.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
