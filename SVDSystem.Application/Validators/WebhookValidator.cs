using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Validators;

/// <summary>
/// Validates Azure DevOps webhook payloads to ensure required fields are present
/// </summary>
public static class WebhookValidator
{
    /// <summary>
    /// Validates that a Pull Request Created webhook event contains all required fields
    /// </summary>
    /// <param name="payload">The webhook payload to validate</param>
    /// <returns>Tuple containing validation result and error message if invalid</returns>
    public static (bool IsValid, string? ErrorMessage) ValidatePullRequestCreated(PullRequestCreatedEvent? payload)
    {
        // Check payload exists
        if (payload == null)
            return (false, "Payload is null");

        // Check event ID
        if (string.IsNullOrWhiteSpace(payload.Id))
            return (false, "Event ID is required");

        // Check resource exists
        if (payload.Resource == null)
            return (false, "Resource is required");

        var resource = payload.Resource;

        // Check pull request ID
        if (resource.PullRequestId <= 0)
            return (false, "Pull request ID is required and must be positive");

        // Check title
        if (string.IsNullOrWhiteSpace(resource.Title))
            return (false, "Pull request title is required");

        // Check source and target branches
        if (string.IsNullOrWhiteSpace(resource.SourceRefName))
            return (false, "Source branch (SourceRefName) is required");

        if (string.IsNullOrWhiteSpace(resource.TargetRefName))
            return (false, "Target branch (TargetRefName) is required");

        // Check status
        if (string.IsNullOrWhiteSpace(resource.Status))
            return (false, "Pull request status is required");

        // Check repository exists
        if (resource.Repository == null)
            return (false, "Repository information is required");

        var repository = resource.Repository;

        // Check repository fields
        if (string.IsNullOrWhiteSpace(repository.Id))
            return (false, "Repository ID is required");

        if (string.IsNullOrWhiteSpace(repository.Name))
            return (false, "Repository name is required");

        if (string.IsNullOrWhiteSpace(repository.Url))
            return (false, "Repository URL is required");

        // Check project exists
        if (repository.Project == null)
            return (false, "Project information is required");

        var project = repository.Project;

        // Check project fields
        if (string.IsNullOrWhiteSpace(project.Id))
            return (false, "Project ID is required");

        if (string.IsNullOrWhiteSpace(project.Name))
            return (false, "Project name is required");

        if (string.IsNullOrWhiteSpace(project.Url))
            return (false, "Project URL is required");

        // All validations passed
        return (true, null);
    }
}
