namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// A named group of ignore-path prefixes and selected file-type extensions
/// built from the <see cref="FileTypeFilter"/> master list.
/// </summary>
public class FilterGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name shown in the frontend selector.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of path prefixes to ignore (e.g. "tests/,docs/").
    /// </summary>
    public string IgnorePaths { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of file extensions selected from the master list (e.g. ".cs,.sql,.py").
    /// </summary>
    public string FileTypeExtensions { get; set; } = string.Empty;

    /// <summary>
    /// The user who created this group.
    /// </summary>
    public User? CreatedBy { get; set; }
}
