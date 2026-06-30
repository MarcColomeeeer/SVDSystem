namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// A named group of vulnerability categories that can be applied to repository configurations.
/// </summary>
public class CategoryGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name shown in the frontend selector.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of vulnerability category names.
    /// </summary>
    public string Categories { get; set; } = string.Empty;

    /// <summary>
    /// The user who created this group.
    /// </summary>
    public User? CreatedBy { get; set; }
}
