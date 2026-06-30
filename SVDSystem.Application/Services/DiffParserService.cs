using System.Text.RegularExpressions;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Domain.Entities.Diff;
using SVDSystem.Domain.Entities.Webhook;

namespace SVDSystem.Application.Services;

/// <summary>
/// Service for parsing raw git diff output into structured data models.
/// </summary>
public partial class DiffParserService : IDiffParserService
{
    // Matches: diff --git a/path b/path
    [GeneratedRegex(@"^diff --git a/(.+) b/(.+)$", RegexOptions.Multiline)]
    private static partial Regex FileHeaderRegex();

    // Matches: @@ -5,3 +5,6 @@ (optional trailing context label)
    [GeneratedRegex(@"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@", RegexOptions.Multiline)]
    private static partial Regex HunkHeaderRegex();

    /// <inheritdoc />
    public PullRequestDiff Parse(string rawDiff, PullRequestResource resource, RepositoryConfiguration configuration)
    {
        var result = new PullRequestDiff
        {
            PullRequestId = resource.PullRequestId,
            RepositoryName = resource.Repository?.Name ?? string.Empty,
            SourceBranch = resource.SourceRefName,
            TargetBranch = resource.TargetRefName,
        };

        if (string.IsNullOrWhiteSpace(rawDiff))
        {
            return result;
        }

        var ignorePaths = configuration.GetIgnorePaths();
        var fileTypeFilters = configuration.GetFileTypeFilters();

        // Split into per-file blocks at each "diff --git" header
        // [0] = text before first match (empty/irrelevant), [1] = original path, [2] = modified path, [3] = block body  → repeats
        var fileBlocks = FileHeaderRegex().Split(rawDiff);

        for (int i = 1; i < fileBlocks.Length; i += 3)
        {
            var originalPath = fileBlocks[i].Trim();
            var modifiedPath = fileBlocks[i + 1].Trim();
            var fileBody = fileBlocks[i + 2];

            var isNewFile = originalPath == "/dev/null" || fileBody.Contains("\nnew file mode");
            var isDeletedFile = modifiedPath == "/dev/null" || fileBody.Contains("\ndeleted file mode");

            var fileDiff = new FileDiff
            {
                OriginalPath = isNewFile ? null : originalPath,
                ModifiedPath = isDeletedFile ? null : modifiedPath,
            };

            if (fileDiff.IsNewFile && !configuration.IncludeAddedFiles)
            {
                continue;
            }

            if (fileDiff.IsDeletedFile && !configuration.IncludeDeletedFiles)
            {
                continue;
            }

            if (!fileDiff.IsNewFile && !fileDiff.IsDeletedFile && !configuration.IncludeModifiedFiles)
            {
                continue;
            }

            if (ShouldIgnorePath(fileDiff.DisplayPath, ignorePaths))
            {
                continue;
            }

            if (!PassesFileTypeFilter(fileDiff.DisplayPath, fileTypeFilters))
            {
                continue;
            }

            ParseFileDiff(fileDiff, fileBody);

            result.Files.Add(fileDiff);
        }

        return result;
    }


    /// <summary>
    /// Returns true when the file path starts with any of the configured ignore prefixes.
    /// </summary>
    private static bool ShouldIgnorePath(string filePath, IReadOnlyList<string> ignorePaths)
    {
        if (ignorePaths.Count == 0)
        {
            return false;
        }

        // Normalise path separators so "tests/" matches both "tests/foo.cs" and "tests\foo.cs"
        var normalised = filePath.Replace('\\', '/');

        return ignorePaths.Any(prefix => normalised.StartsWith(prefix.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns true when the file extension is in the whitelist, or the whitelist is empty (= all allowed).
    /// </summary>
    private static bool PassesFileTypeFilter(string filePath, IReadOnlyList<string> fileTypeFilters)
    {
        if (fileTypeFilters.Count == 0)
        {
            return true;
        }

        var extension = Path.GetExtension(filePath);
        
        return fileTypeFilters.Any(ext => string.Equals(ext.TrimStart('.'), extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Populates the hunks of an already-constructed <see cref="FileDiff"/> from the raw file body.
    /// </summary>
    private static void ParseFileDiff(FileDiff fileDiff, string fileBody)
    {
        var hunkMatches = HunkHeaderRegex().Matches(fileBody);

        for (int i = 0; i < hunkMatches.Count; i++)
        {
            var match = hunkMatches[i];

            // Content runs from the end of this header to the start of the next (or EOF)
            var contentStart = match.Index + match.Length;
            var contentEnd = i + 1 < hunkMatches.Count ? hunkMatches[i + 1].Index : fileBody.Length;

            var hunk = new DiffHunk
            {
                OriginalStartLine = int.Parse(match.Groups[1].Value),
                // When the line count is omitted in the header (e.g. @@ -5 +5 @@) it means 1
                OriginalLineCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1,
                ModifiedStartLine = int.Parse(match.Groups[3].Value),
                ModifiedLineCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1,
                Content = match.Value + TrimTrailingNewline(fileBody[contentStart..contentEnd])
            };

            fileDiff.Hunks.Add(hunk);
        }
    }

    /// <summary>
    /// Trim trailing newlines from the hunk content to avoid sending unnecessary blank lines to the analysis server.
    /// </summary>
    private static string TrimTrailingNewline(string text)
    {
        if (text.EndsWith("\r\n"))
        {
            return text[..^2];
        }

        if (text.EndsWith('\n'))
        {
            return text[..^1];
        }

        return text;
    }
}
