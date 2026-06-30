using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Domain.Entities.Diff;
using SVDSystem.Domain.Entities.Vulnerability;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Analyzes pull request diffs for security vulnerabilities using a local Ollama instance.
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Sends all file diffs from a pull request to the vulnerability server.
    /// </summary>
    /// <param name="pullRequestDiff">The parsed diff containing all changed files.</param>
    /// <param name="configuration">The repository configuration used to build the analysis request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The vulnerability analysis result for the pull request.</returns>
    Task<PullRequestAnalysis> AnalyzeAsync(PullRequestDiff pullRequestDiff, RepositoryConfiguration configuration, CancellationToken cancellationToken = default);
}
