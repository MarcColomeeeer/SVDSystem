using System.Text.Json.Serialization;

namespace SVDSystem.Domain.Entities.Webhook;

/// <summary>
/// Contains the detailed pull request information from the webhook event.
/// </summary>
public class PullRequestResource
{
    /// <summary>
    /// Repository information where the PR was created.
    /// </summary>
    [JsonPropertyName("repository")]
    public Repository? Repository { get; set; }

    /// <summary>
    /// The unique identifier of the pull request within the repository.
    /// </summary>
    [JsonPropertyName("pullRequestId")]
    public int PullRequestId { get; set; }

    /// <summary>
    /// Current status of the pull request.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// User who created the pull request.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public Identity? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the pull request was created.
    /// </summary>
    [JsonPropertyName("creationDate")]
    public DateTimeOffset CreationDate { get; set; }

    /// <summary>
    /// Title of the pull request.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the pull request.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Source branch ref name (where changes are coming from).
    /// </summary>
    [JsonPropertyName("sourceRefName")]
    public string SourceRefName { get; set; } = string.Empty;

    /// <summary>
    /// Target branch ref name (where changes will be merged to).
    /// </summary>
    [JsonPropertyName("targetRefName")]
    public string TargetRefName { get; set; } = string.Empty;

    /// <summary>
    /// Merge status of the pull request.
    /// </summary>
    [JsonPropertyName("mergeStatus")]
    public string MergeStatus { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the merge operation.
    /// </summary>
    [JsonPropertyName("mergeId")]
    public string MergeId { get; set; } = string.Empty;

    /// <summary>
    /// The latest commit on the source branch.
    /// </summary>
    [JsonPropertyName("lastMergeSourceCommit")]
    public Commit? LastMergeSourceCommit { get; set; }

    /// <summary>
    /// The latest commit on the target branch.
    /// </summary>
    [JsonPropertyName("lastMergeTargetCommit")]
    public Commit? LastMergeTargetCommit { get; set; }

    /// <summary>
    /// The merge commit (if merge was attempted).
    /// </summary>
    [JsonPropertyName("lastMergeCommit")]
    public Commit? LastMergeCommit { get; set; }

    /// <summary>
    /// List of reviewers assigned to the pull request.
    /// </summary>
    [JsonPropertyName("reviewers")]
    public List<Reviewer> Reviewers { get; set; } = [];

    /// <summary>
    /// URL to access the pull request in Azure DevOps.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}