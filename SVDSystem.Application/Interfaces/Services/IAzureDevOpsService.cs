using SVDSystem.Domain.Entities.Vulnerability;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Service for interacting with the Azure DevOps REST API.
/// </summary>
public interface IAzureDevOpsService
{
    /// <summary>
    /// Posts one comment thread per vulnerable file to a pull request,
    /// including all findings for that file as separate comments within the same thread.
    /// </summary>
    /// <param name="organizationUrl">Azure DevOps organization base URL (e.g. https://dev.azure.com/MyOrg).</param>
    /// <param name="project">Project name or ID.</param>
    /// <param name="repositoryId">Repository GUID.</param>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <param name="analysis">Vulnerability analysis result containing all findings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PostPullRequestAnalysisCommentsAsync(
        string organizationUrl,
        string project,
        string repositoryId,
        int pullRequestId,
        PullRequestAnalysis analysis,
        CancellationToken cancellationToken = default);
}
