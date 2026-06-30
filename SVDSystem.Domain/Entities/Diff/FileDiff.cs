namespace SVDSystem.Domain.Entities.Diff;

/// <summary>
/// Represents the diff of a single file within a pull request.
/// Corresponds to one "diff --git a/... b/..." block in the raw git diff output.
/// </summary>
public class FileDiff
{
    /// <summary>
    /// Unique identifier for this file diff
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Path of the file in the original (before) version.
    /// Null if the file was newly created.
    /// </summary>
    public string? OriginalPath { get; set; }

    /// <summary>
    /// Path of the file in the modified (after) version.
    /// Null if the file was deleted.
    /// </summary>
    public string? ModifiedPath { get; set; }

    /// <summary>
    /// The file path to display (prefers ModifiedPath, falls back to OriginalPath)
    /// </summary>
    public string DisplayPath
    {
        get => ModifiedPath ?? OriginalPath ?? string.Empty;
    }

    /// <summary>
    /// Whether the file was newly added (did not exist before)
    /// </summary>
    public bool IsNewFile => OriginalPath == null;

    /// <summary>
    /// Whether the file was deleted
    /// </summary>
    public bool IsDeletedFile => ModifiedPath == null;

    /// <summary>
    /// Whether the file was renamed
    /// </summary>
    public bool IsRenamedFile
    {
        get => OriginalPath != null && ModifiedPath != null && OriginalPath != ModifiedPath;
    }

    /// <summary>
    /// All hunks (change blocks) in this file diff
    /// </summary>
    public List<DiffHunk> Hunks { get; set; } = new List<DiffHunk>();
}
