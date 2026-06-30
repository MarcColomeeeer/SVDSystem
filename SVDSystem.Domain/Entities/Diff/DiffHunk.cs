namespace SVDSystem.Domain.Entities.Diff;

/// <summary>
/// Represents a single hunk (contiguous block of changes) within a file diff.
/// </summary>
public class DiffHunk
{
    /// <summary>
    /// Starting line number in the original (before) file
    /// </summary>
    public int OriginalStartLine { get; set; }

    /// <summary>
    /// Number of lines shown from the original file
    /// </summary>
    public int OriginalLineCount { get; set; }

    /// <summary>
    /// Starting line number in the modified (after) file
    /// </summary>
    public int ModifiedStartLine { get; set; }

    /// <summary>
    /// Number of lines shown in the modified file
    /// </summary>
    public int ModifiedLineCount { get; set; }

    /// <summary>
    /// The raw content of this hunk, including the @@ header line.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
