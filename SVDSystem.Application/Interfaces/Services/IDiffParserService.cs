using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Domain.Entities.Diff;
using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Parses raw git diff output into structured <see cref="FileDiff"/> objects.
/// </summary>
public interface IDiffParserService
{
    /// <summary>
    /// Parses a raw git diff string into a <see cref="PullRequestDiff"/> with one <see cref="FileDiff"/> per changed file.
    /// </summary>
    /// <param name="rawDiff">The raw git diff string.</param>
    /// <param name="resource">The pull request resource from the webhook event.</param>
    /// <param name="configuration">The repository configuration.</param>
    /// <returns>A <see cref="PullRequestDiff"/> object containing the parsed file diffs.</returns>
    PullRequestDiff Parse(string rawDiff, PullRequestResource resource, RepositoryConfiguration configuration);
}
