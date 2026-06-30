using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Service for processing Azure DevOps webhook events.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Processes a Pull Request Created webhook event from Azure DevOps.
    /// </summary>
    /// <param name="webhookEvent">The pull request created event from Azure DevOps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProcessPullRequestCreatedAsync(PullRequestCreatedEvent webhookEvent, CancellationToken cancellationToken = default);
}
