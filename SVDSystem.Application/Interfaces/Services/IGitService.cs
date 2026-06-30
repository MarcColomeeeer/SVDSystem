using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Service for Git operations including repository management, fetching, and diff generation.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Ensures a repository exists locally and returns its path.
    /// </summary>
    /// <param name="repository">Repository information from the webhook payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Full path to the local repository, or null if unavailable.</returns>
    Task<string?> EnsureRepositoryAsync(Repository repository, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches latest changes from all remotes.
    /// </summary>
    /// <param name="repositoryPath">Full path to the local git repository.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task FetchAsync(string repositoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Intelligently gets the diff for a pull request.
    /// </summary>
    /// <param name="repositoryPath">Full path to the local git repository.</param>
    /// <param name="resource">Pull request resource from webhook.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Git diff output as string.</returns>
    Task<string> GetPullRequestDiffAsync(string repositoryPath, PullRequestResource resource, CancellationToken cancellationToken = default);
}
