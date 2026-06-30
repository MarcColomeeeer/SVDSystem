using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Domain.Entities.Diff;
using SVDSystem.Domain.Entities.Vulnerability;
using SVDSystem.Infrastructure.Configuration;
using SVDSystem.Infrastructure.Dtos;

namespace SVDSystem.Infrastructure.Services;

/// <summary>
/// Analyzes pull-request diffs for security vulnerabilities using a locally running Ollama instance.
/// Each hunk is sent as a separate chat request to keep prompts within the model's context window.
/// </summary>
public class OllamaService : IOllamaService
{
    private const string ChatEndpoint = "/api/chat";

    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(HttpClient httpClient, IOptions<OllamaSettings> settings, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PullRequestAnalysis> AnalyzeAsync(
        PullRequestDiff pullRequestDiff,
        RepositoryConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var analysis = new PullRequestAnalysis
        {
            PullRequestDiffId = pullRequestDiff.Id,
            PullRequestId = pullRequestDiff.PullRequestId
        };

        var files = pullRequestDiff.Files;

        if (files.Count == 0)
        {
            _logger.LogInformation(
                "PR #{PullRequestId}: no files to analyze — skipping.",
                pullRequestDiff.PullRequestId);

            return analysis;
        }

        var systemPrompt = BuildSystemPrompt(configuration);

        _logger.LogInformation(
            "Analyzing PR #{PullRequestId} via Ollama ({Model}): {FileCount} file(s).",
            pullRequestDiff.PullRequestId,
            _settings.Model,
            files.Count);

        foreach (var file in files)
        {
            var findings = new List<VulnerabilityFinding>();

            foreach (var hunk in file.Hunks)
            {
                var hunkFindings = await AnalyzeHunkAsync(file, hunk, systemPrompt, configuration.SeverityThreshold, cancellationToken);
                findings.AddRange(hunkFindings);
            }

            if (findings.Count > 0)
            {
                analysis.Results.Add(new FileDiffAnalysis
                {
                    FileDiffId = file.Id,
                    FilePath = file.DisplayPath,
                    Findings = findings
                });
            }
        }

        _logger.LogInformation(
            "Ollama analysis complete for PR #{PullRequestId}. Vulnerable files: {Count}.",
            pullRequestDiff.PullRequestId,
            analysis.Results.Count);

        return analysis;
    }

    private async Task<List<VulnerabilityFinding>> AnalyzeHunkAsync(
        FileDiff file,
        DiffHunk hunk,
        string systemPrompt,
        VulnerabilityLevel severityThreshold,
        CancellationToken cancellationToken)
    {
        var userMessage =
            $"File: {file.DisplayPath}\n\n" +
            $"```diff\n{hunk.Content}\n```";

        var request = new OllamaChatRequest
        {
            Model = _settings.Model,
            Stream = false,
            Messages =
            [
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage { Role = "user",   Content = userMessage }
            ]
        };

        OllamaChatResponse? response;
        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync(ChatEndpoint, request, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            response = await httpResponse.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama request failed for file {FilePath}.", file.DisplayPath);
            return [];
        }

        var content = response?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
            return [];

        return ParseFindings(content, severityThreshold);
    }

    // ── Prompt builder ───────────────────────────────────────────────────────

    private static string BuildSystemPrompt(RepositoryConfiguration configuration)
    {
        var basePrompt = configuration.UseCategories
            ? SystemPrompts.CategoryPromptContent
                .Replace("{VULNERABILITY_CATEGORIES}", string.Join(", ", configuration.GetVulnerabilityCategories()))
            : SystemPrompts.GeneralPromptContent;

        return string.IsNullOrWhiteSpace(configuration.CustomPrompt)
            ? basePrompt
            : basePrompt + "\n\nAdditional instructions:\n" + configuration.CustomPrompt;
    }

    // ── Response parser ──────────────────────────────────────────────────────

    private List<VulnerabilityFinding> ParseFindings(string content, VulnerabilityLevel threshold)
    {
        // The model is prompted to return a JSON array of findings.
        // Extract the first JSON array from the response text.
        var start = content.IndexOf('[');
        var end   = content.LastIndexOf(']');
        if (start < 0 || end <= start)
            return [];

        var json = content[start..(end + 1)];

        List<OllamaFinding>? raw;
        try
        {
            raw = JsonSerializer.Deserialize<List<OllamaFinding>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Ollama findings JSON.");
            return [];
        }

        if (raw is null || raw.Count == 0)
            return [];

        return raw
            .Select(f => (finding: f, level: ParseLevel(f.Level)))
            .Where(x => x.level.HasValue && x.level.Value >= threshold)
            .Select(x => new VulnerabilityFinding
            {
                Level             = x.level!.Value,
                VulnerabilityType = x.finding.VulnerabilityType,
                Comment           = x.finding.Comment
            })
            .ToList();
    }

    private static VulnerabilityLevel? ParseLevel(string level) =>
        Enum.TryParse<VulnerabilityLevel>(level, ignoreCase: true, out var result) ? result : null;
}
