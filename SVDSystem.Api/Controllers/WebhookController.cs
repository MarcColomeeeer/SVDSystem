using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Application.Validators;
using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Api.Controllers;

/// <summary>
/// Handles webhook events from Azure DevOps.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Receives and processes Pull Request Created webhook events from Azure DevOps.
    /// </summary>
    /// <param name="payload">The webhook payload from Azure DevOps</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK if successful, 400 Bad Request if invalid payload, 500 on errors</returns>
    [HttpPost("pr-created")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OnPullRequestCreated([FromBody] PullRequestCreatedEvent payload, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that all required fields are present
            var (isValid, errorMessage) = WebhookValidator.ValidatePullRequestCreated(payload);

            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook payload: {Error}", errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            // Validate event type matches expected value
            if (payload.EventType != "git.pullrequest.created")
            {
                _logger.LogWarning("Unexpected event type: {EventType}", payload.EventType);
                return BadRequest(new { error = $"Unexpected event type: {payload.EventType}" });
            }

            _logger.LogInformation(
                "Received PR #{PullRequestId} '{Title}' in {Project}/{Repository}",
                payload.Resource!.PullRequestId,
                payload.Resource.Title,
                payload.Resource.Repository!.Project!.Name,
                payload.Resource.Repository.Name);

            // Process the webhook
            await _webhookService.ProcessPullRequestCreatedAsync(payload, cancellationToken);

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred processing the webhook" });
        }
    }

    /// <summary>
    /// Health check endpoint to verify the webhook service is running.
    /// </summary>
    /// <returns>200 OK with status and timestamp</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow,
            service = "Azure DevOps Webhook Handler"
        });
    }
}