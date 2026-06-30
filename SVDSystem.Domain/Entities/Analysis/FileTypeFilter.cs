namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// A single named file type in the master list
/// </summary>
public class FileTypeFilter
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Human-readable display name, e.g. "SQL (.sql)".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// File extension used for actual filtering, e.g. ".sql".
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// The user who created this entry.
    /// </summary>
    public User? CreatedBy { get; set; }
}
