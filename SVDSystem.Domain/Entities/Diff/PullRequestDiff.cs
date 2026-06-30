namespace SVDSystem.Domain.Entities.Diff;

/// <summary>
/// Represents the complete parsed diff for a pull request.
/// Contains one FileDiff per changed file.
/// </summary>
public class PullRequestDiff
{
    /// <summary>
    /// Unique identifier for this diff result
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Azure DevOps pull request ID
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// Repository name
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Source branch reference (e.g. refs/heads/feature/my-feature)
    /// </summary>
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>
    /// Target branch reference (e.g. refs/heads/main)
    /// </summary>
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>
    /// One entry per changed file in the pull request
    /// </summary>
    public List<FileDiff> Files { get; set; } = new List<FileDiff>();

    /// <summary>
    /// Total files changed
    /// </summary>
    public int TotalFilesChanged => Files.Count;
}
