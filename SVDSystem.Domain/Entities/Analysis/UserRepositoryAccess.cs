namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// Access permission relationship between a user and a repository configuration.
/// </summary>
public class UserRepositoryAccess
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user being granted access.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the repository configuration this user can access.
    /// </summary>
    public RepositoryConfiguration? RepositoryConfiguration { get; set; }
}
