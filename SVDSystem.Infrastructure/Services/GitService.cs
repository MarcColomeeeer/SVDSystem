using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Webhook;
using SVDSystem.Infrastructure.Configuration;

namespace SVDSystem.Infrastructure.Services;

/// <summary>
/// Service for executing git operations on local repositories.
/// Handles cloning, fetching, diff generation, and automatic repository management.
/// </summary>
public class GitService : IGitService
{
    private readonly GitSettings _settings;
    private readonly ILogger<GitService> _logger;
    private readonly SemaphoreSlim _cloneLock = new(1, 1);

    public GitService(
        IOptions<GitSettings> settings,
        ILogger<GitService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    #region Diff Operations

    /// <summary>
    /// Gets the diff between two commits using git diff command.
    /// </summary>
    private async Task<string> GetDiffAsync(string repositoryPath, string sourceCommit, string targetCommit, CancellationToken cancellationToken = default)
    {
        ValidateRepositoryPath(repositoryPath);

        _logger.LogInformation(
            "Getting diff between commits {SourceCommit} and {TargetCommit} in repository {RepositoryPath}",
            sourceCommit, targetCommit, repositoryPath);

        var arguments = $"diff {targetCommit}..{sourceCommit}";
        return await ExecuteGitCommandAsync(repositoryPath, arguments, cancellationToken);
    }

    /// <summary>
    /// Gets the diff between two branches.
    /// </summary>
    private async Task<string> GetDiffBetweenBranchesAsync(
        string repositoryPath,
        string sourceBranch,
        string targetBranch,
        CancellationToken cancellationToken = default)
    {
        ValidateRepositoryPath(repositoryPath);

        // Clean branch names (remove refs/heads/ prefix if present)
        var cleanSourceBranch = CleanBranchName(sourceBranch);
        var cleanTargetBranch = CleanBranchName(targetBranch);

        _logger.LogInformation(
            "Getting diff between branches {SourceBranch} and {TargetBranch} in repository {RepositoryPath}",
            cleanSourceBranch, cleanTargetBranch, repositoryPath);

        var arguments = $"diff {cleanTargetBranch}...{cleanSourceBranch}";
        return await ExecuteGitCommandAsync(repositoryPath, arguments, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetPullRequestDiffAsync(
        string repositoryPath,
        PullRequestResource resource,
        CancellationToken cancellationToken = default)
    {
        ValidateRepositoryPath(repositoryPath);

        // Prefer commit-based diff (immutable and reliable)
        if (resource.LastMergeSourceCommit != null && resource.LastMergeTargetCommit != null)
        {
            var sourceCommit = resource.LastMergeSourceCommit.CommitId;
            var targetCommit = resource.LastMergeTargetCommit.CommitId;

            return await GetDiffAsync(repositoryPath, sourceCommit, targetCommit, cancellationToken);
        }

        // Fallback to branch-based diff
        return await GetDiffBetweenBranchesAsync(
            repositoryPath,
            resource.SourceRefName,
            resource.TargetRefName,
            cancellationToken);
    }

    #endregion

    #region Repository Management

    /// <inheritdoc/>
    public async Task FetchAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        ValidateRepositoryPath(repositoryPath);

        _logger.LogInformation("Fetching latest changes for repository {RepositoryPath}", repositoryPath);

        var arguments = "fetch --all --prune";
        await ExecuteGitCommandAsync(repositoryPath, arguments, cancellationToken);

        _logger.LogInformation("Successfully fetched latest changes");
    }

    /// <summary>
    /// Clones a repository from remote URL to local path.
    /// </summary>
    private async Task CloneAsync(string remoteUrl, string localPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            throw new ArgumentException("Remote URL cannot be null or empty", nameof(remoteUrl));
        }

        if (string.IsNullOrWhiteSpace(localPath))
        {
            throw new ArgumentException("Local path cannot be null or empty", nameof(localPath));
        }

        if (Directory.Exists(localPath))
        {
            var gitDir = Path.Combine(localPath, ".git");
            if (Directory.Exists(gitDir))
            {
                _logger.LogInformation("Repository already exists at {LocalPath}, skipping clone", localPath);
                return;
            }
        }

        _logger.LogInformation("Cloning repository from {RemoteUrl} to {LocalPath}", remoteUrl, localPath);

        // Ensure parent directory exists
        var parentDir = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }

        var arguments = $"clone {remoteUrl} \"{localPath}\"";

        // Use clone timeout instead of regular command timeout
        var originalTimeout = _settings.CommandTimeoutSeconds;
        try
        {
            await ExecuteGitCommandAsync(
                parentDir ?? Environment.CurrentDirectory,
                arguments,
                cancellationToken,
                timeoutSeconds: _settings.CloneTimeoutSeconds);

            _logger.LogInformation("Successfully cloned repository to {LocalPath}", localPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone repository from {RemoteUrl}", remoteUrl);

            // Clean up partial clone if it exists
            if (Directory.Exists(localPath))
            {
                try
                {
                    Directory.Delete(localPath, recursive: true);
                    _logger.LogInformation("Cleaned up partial clone at {LocalPath}", localPath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to clean up partial clone at {LocalPath}", localPath);
                }
            }

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> EnsureRepositoryAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        var repositoryId = repository.Id;
        var projectName = repository.Project?.Name ?? string.Empty;
        var repositoryName = repository.Name;
        var remoteUrl = repository.RemoteUrl;

        // First, check if there's a manually configured path
        if (_settings.RepositoryPaths.TryGetValue(repositoryId, out var configuredPath))
        {
            _logger.LogDebug("Using configured path for repository {RepositoryId}: {Path}", repositoryId, configuredPath);

            if (!Directory.Exists(configuredPath))
            {
                _logger.LogWarning("Configured path does not exist: {Path}", configuredPath);
                return null;
            }

            return configuredPath;
        }

        // If no base path configured, can't auto-clone
        if (string.IsNullOrWhiteSpace(_settings.BaseRepositoryPath))
        {
            _logger.LogWarning(
                "Repository {RepositoryId} not found and BaseRepositoryPath is not configured. Cannot auto-clone.",
                repositoryId);
            return null;
        }

        // Generate path: BaseRepositoryPath\ProjectName\RepositoryName
        var sanitizedProjectName = SanitizeDirectoryName(projectName);
        var sanitizedRepoName = SanitizeDirectoryName(repositoryName);
        var repositoryPath = Path.Combine(_settings.BaseRepositoryPath, sanitizedProjectName, sanitizedRepoName);

        _logger.LogDebug(
            "Generated repository path: {RepositoryPath} for Project: {ProjectName}, Repo: {RepositoryName}",
            repositoryPath, projectName, repositoryName);

        // Use semaphore to prevent concurrent clones of the same repository
        await _cloneLock.WaitAsync(cancellationToken);
        try
        {
            // Check if repository already exists
            if (Directory.Exists(repositoryPath))
            {
                var gitDir = Path.Combine(repositoryPath, ".git");
                if (Directory.Exists(gitDir))
                {
                    _logger.LogInformation(
                        "Repository {ProjectName}\\{RepositoryName} already exists at {Path}",
                        projectName,
                        repositoryName,
                        repositoryPath);
                    return repositoryPath;
                }
                else
                {
                    _logger.LogWarning(
                        "Directory exists but is not a git repository: {Path}. Will attempt to clone.",
                        repositoryPath);

                    // Clean up invalid directory
                    try
                    {
                        Directory.Delete(repositoryPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to clean up invalid directory: {Path}", repositoryPath);
                        return null;
                    }
                }
            }

            // Auto-clone if enabled
            if (!_settings.AutoCloneRepositories)
            {
                _logger.LogWarning(
                    "Repository {RepositoryName} does not exist locally and AutoCloneRepositories is disabled",
                    repositoryName);
                return null;
            }

            _logger.LogInformation(
                "Repository {ProjectName}\\{RepositoryName} not found locally. Cloning from {RemoteUrl}...",
                projectName,
                repositoryName,
                remoteUrl);

            await CloneAsync(remoteUrl, repositoryPath, cancellationToken);

            return repositoryPath;
        }
        finally
        {
            _cloneLock.Release();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Executes a git command and returns the output.
    /// Handles timeouts, error capturing, and proper process cleanup.
    /// </summary>
    private async Task<string> ExecuteGitCommandAsync(
        string workingDirectory,
        string arguments,
        CancellationToken cancellationToken,
        int? timeoutSeconds = null)
    {
        var timeout = timeoutSeconds ?? _settings.CommandTimeoutSeconds;

        var processStartInfo = new ProcessStartInfo
        {
            FileName = _settings.GitExecutablePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                outputBuilder.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                errorBuilder.AppendLine(args.Data);
            }
        };

        _logger.LogDebug("Executing git command: {Command} {Arguments}", _settings.GitExecutablePath, arguments);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout), cancellationToken);
        var processTask = process.WaitForExitAsync(cancellationToken);

        var completedTask = await Task.WhenAny(processTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }

            throw new TimeoutException($"Git command timed out after {timeout} seconds: {arguments}");
        }

        await processTask;

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (process.ExitCode != 0)
        {
            _logger.LogError(
                "Git command failed with exit code {ExitCode}. Command: {Command}. Error: {Error}",
                process.ExitCode, arguments, error);

            throw new InvalidOperationException($"Git command failed with exit code {process.ExitCode}: {error}");
        }

        _logger.LogDebug("Git command completed successfully");

        return output;
    }

    /// <summary>
    /// Validates that a path exists and is a valid git repository.
    /// Throws exceptions if path is invalid, doesn't exist, or is not a git repository.
    /// </summary>
    private void ValidateRepositoryPath(string repositoryPath)
    {
        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            throw new ArgumentException("Repository path cannot be null or empty", nameof(repositoryPath));
        }

        if (!Directory.Exists(repositoryPath))
        {
            throw new DirectoryNotFoundException(
                $"Repository directory not found: {repositoryPath}");
        }

        var gitDirectory = Path.Combine(repositoryPath, ".git");
        if (!Directory.Exists(gitDirectory))
        {
            throw new InvalidOperationException(
                $"Directory is not a git repository: {repositoryPath}");
        }
    }

    /// <summary>
    /// Cleans a branch name by removing the refs/heads/ prefix if present.
    /// Example: "refs/heads/main" -> "main"
    /// </summary>
    private static string CleanBranchName(string branchName)
    {
        // Remove refs/heads/ prefix if present
        if (branchName.StartsWith("refs/heads/"))
        {
            return branchName.Substring("refs/heads/".Length);
        }

        return branchName;
    }

    /// <summary>
    /// Sanitizes a string to be used as a directory name.
    /// Removes invalid path characters and replaces spaces with underscores.
    /// </summary>
    private static string SanitizeDirectoryName(string name)
    {
        // Remove invalid characters from directory name
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Remove leading/trailing dots and spaces
        sanitized = sanitized.Trim('.', ' ');

        // Ensure it's not empty
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "repository";
        }

        return sanitized;
    }

    #endregion
}
