using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Vulnerability;
using SVDSystem.Infrastructure.Configuration;

namespace SVDSystem.Infrastructure.Services;

/// <summary>
/// Posts comments to Azure DevOps pull request threads using the REST API.
/// </summary>
public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly HttpClient _httpClient;
    private readonly GitSettings _gitSettings;
    private readonly ILogger<AzureDevOpsService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AzureDevOpsService(HttpClient httpClient, IOptions<GitSettings> gitSettings, ILogger<AzureDevOpsService> logger)
    {
        _httpClient = httpClient;
        _gitSettings = gitSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PostPullRequestAnalysisCommentsAsync(
        string organizationUrl,
        string project,
        string repositoryId,
        int pullRequestId,
        PullRequestAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        var threadsUrl = BuildThreadsUrl(organizationUrl, project, repositoryId, pullRequestId);
        var token = BuildAuthToken(_gitSettings.AzureDevOpsPersonalAccessToken);

        foreach (var result in analysis.Results)
        {
            await PostFileThreadAsync(threadsUrl, token, result.FilePath, result.Findings, pullRequestId, cancellationToken);
        }

        _logger.LogInformation(
            "Posted {Count} vulnerability thread(s) to PR #{PullRequestId}.",
            analysis.Results.Count,
            pullRequestId);
    }

    /// <summary>
    /// Builds the PR threads API URL.
    /// e.g. https://dev.azure.com/Org/Project/_apis/git/repositories/{repoId}/pullRequests/{prId}/threads?api-version=7.1
    /// </summary>
    private static string BuildThreadsUrl(string organizationUrl, string project, string repositoryId, int pullRequestId) =>
        $"{organizationUrl.TrimEnd('/')}/{project}/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/threads?api-version=7.1";

    /// <summary>
    /// Builds the Basic Auth header value from a PAT.
    /// </summary>
    private static string BuildAuthToken(string pat) =>
        Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));

    /// <summary>
    /// Posts a single thread for a file containing one comment per finding.
    /// </summary>
    private async Task PostFileThreadAsync(
        string threadsUrl,
        string authToken,
        string filePath,
        IReadOnlyList<VulnerabilityFinding> findings,
        int pullRequestId,
        CancellationToken cancellationToken)
    {
        var comments = findings.Select((f, index) => new
        {
            parentCommentId = index == 0 ? 0 : 1,
            content = $"⚠️ **{f.VulnerabilityType}** — Severity: `{f.Level}`\n\n{f.Comment}",
            commentType = 1
        }).ToArray();

        var body = new
        {
            comments,
            status = 1,
            threadContext = new { filePath }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, threadsUrl)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        _logger.LogInformation(
            "Posting vulnerability thread to PR #{PullRequestId}, file: {FilePath} ({Count} finding(s))",
            pullRequestId,
            filePath,
            findings.Count);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to post PR thread. Status: {StatusCode}, Body: {Body}",
                response.StatusCode,
                errorBody);
        }
    }
}
