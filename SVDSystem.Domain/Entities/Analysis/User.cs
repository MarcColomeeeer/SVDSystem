namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// Represents an Entra ID user that has interacted with the system.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Azure Entra ID object ID. Unique across all users.
    /// </summary>
    public string ObjectId { get; set; } = string.Empty;

    /// <summary>
    /// Display name cached from Entra. Updated on each login.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Email address cached from Entra. Updated on each login.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
