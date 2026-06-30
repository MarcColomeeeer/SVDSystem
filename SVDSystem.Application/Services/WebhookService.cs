using Microsoft.Extensions.Logging;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Services;

/// <summary>
/// Orchestrates the full webhook processing pipeline:
/// clone/fetch → diff → parse → vulnerability analysis → (future: PR comments)
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IGitService _gitService;
    private readonly IDiffParserService _diffParser;
    private readonly IOllamaService _vulnerabilityService;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly IRepositoryConfigurationRepository _configurationRepository;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IGitService gitService,
        IDiffParserService diffParser,
        IOllamaService vulnerabilityService,
        IAzureDevOpsService azureDevOpsService,
        IRepositoryConfigurationRepository configurationRepository,
        ILogger<WebhookService> logger)
    {
        _gitService = gitService;
        _diffParser = diffParser;
        _vulnerabilityService = vulnerabilityService;
        _azureDevOpsService = azureDevOpsService;
        _configurationRepository = configurationRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessPullRequestCreatedAsync(PullRequestCreatedEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        var resource = webhookEvent.Resource!;
        var repository = resource.Repository!;
        var project = repository.Project!;

        _logger.LogInformation(
            "Processing PR #{PullRequestId}: '{Title}' in {Project}/{Repository}",
            resource.PullRequestId,
            resource.Title,
            project.Name,
            repository.Name);

        // Step 1: Resolve (or auto-create) repository configuration
        var configuration = await GetOrCreateConfigurationAsync(
            repository.Id,
            repository.Name,
            project.Name,
            repository.RemoteUrl,
            cancellationToken);

        if (!configuration.Enabled)
        {
            _logger.LogInformation(
                "PR #{PullRequestId}: repository '{Repository}' is disabled — skipping.",
                resource.PullRequestId,
                repository.Name);
            return;
        }

        // Step 2: Ensure repository exists locally (auto-clone if needed)
        var repositoryPath = await _gitService.EnsureRepositoryAsync(repository, cancellationToken);

        if (string.IsNullOrEmpty(repositoryPath))
        {
            _logger.LogError(
                "Failed to obtain repository path for PR #{PullRequestId}. Cannot process.",
                resource.PullRequestId);
            return;
        }

        // Step 3: Fetch latest changes from Azure DevOps
        await _gitService.FetchAsync(repositoryPath, cancellationToken);

        // Step 4: Get raw git diff
        var rawDiff = await _gitService.GetPullRequestDiffAsync(repositoryPath, resource, cancellationToken);

        // Step 5: Parse the raw diff into structured per-file diffs
        var pullRequestDiff = _diffParser.Parse(rawDiff, resource, configuration);

        // Step 6: Send file diffs to the vulnerability analysis server (single HTTP request)
        var analysis = await _vulnerabilityService.AnalyzeAsync(pullRequestDiff, configuration, cancellationToken);

        // Step 7: Post comments back to the PR for each vulnerable file found
        if (analysis.HasVulnerabilities)
        {
            var organizationUrl = ExtractOrganizationUrl(repository.RemoteUrl);

            await _azureDevOpsService.PostPullRequestAnalysisCommentsAsync(
                organizationUrl, project.Name, repository.Id, resource.PullRequestId, analysis, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "PR #{PullRequestId}: no vulnerabilities found — no comments posted.",
                resource.PullRequestId);
        }
    }

    /// <summary>
    /// Extracts the organization base URL from an Azure DevOps remote URL.
    /// e.g. https://MyOrg@dev.azure.com/MyOrg/Project/_git/Repo → https://dev.azure.com/MyOrg
    /// </summary>
    private static string ExtractOrganizationUrl(string remoteUrl)
    {
        var uri = new Uri(remoteUrl);
        var host = uri.Host;
        var segments = uri.AbsolutePath.TrimStart('/').Split('/');
        var org = segments[0];
        return $"https://{host}/{org}";
    }

    /// <summary>
    /// Returns the existing repository configuration from the DB, or creates and persists a default one on the first webhook received.
    /// </summary>
    private async Task<RepositoryConfiguration> GetOrCreateConfigurationAsync(
        string repositoryId,
        string repositoryName,
        string projectName,
        string remoteUrl,
        CancellationToken cancellationToken)
    {
        var existing = await _configurationRepository.GetByRepositoryIdAsync(repositoryId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        _logger.LogInformation(
            "First webhook for repository '{Repository}' (ID: {RepositoryId}) — creating default configuration.",
            repositoryName,
            repositoryId);

        var configuration = new RepositoryConfiguration
        {
            RepositoryId = repositoryId,
            RepositoryName = repositoryName,
            ProjectName = projectName,
            RemoteUrl = remoteUrl
        };

        await _configurationRepository.AddAsync(configuration, cancellationToken);

        return configuration;
    }
}
