namespace SVDSystem.Infrastructure.Configuration;

public class GitSettings
{
    public const string SectionName = "Git";

    /// <summary>
    /// Path to git executable. If empty, assumes git is in PATH
    /// </summary>
    public string GitExecutablePath { get; set; } = "git";

    /// <summary>
    /// Timeout for git operations in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Base directory where all repositories will be stored
    /// Structure: {BaseRepositoryPath}\{ProjectName}\{RepositoryName}
    /// Example: C:\DevOpsRepos\SFQ\SFQ
    /// </summary>
    public string BaseRepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Enable automatic cloning of repositories when they don't exist locally
    /// </summary>
    public bool AutoCloneRepositories { get; set; } = true;

    /// <summary>
    /// Automatically fetch latest changes before getting diff
    /// </summary>
    public bool AutoFetchBeforeDiff { get; set; } = true;

    /// <summary>
    /// Timeout for clone operations in seconds (can be longer than regular commands)
    /// </summary>
    public int CloneTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Azure DevOps Personal Access Token (PAT) for cloning private repositories.
    /// </summary>
    public string AzureDevOpsPersonalAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Dictionary mapping repository IDs to local paths (overrides auto-clone)
    /// Use this for repositories that need custom locations
    /// Key: Azure DevOps Repository ID (GUID)
    /// Value: Local path to the repository
    /// </summary>
    public Dictionary<string, string> RepositoryPaths { get; set; } = new();
}
