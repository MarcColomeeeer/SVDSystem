using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="UserRepositoryAccess"/> records.
/// </summary>
public interface IUserRepositoryAccessRepository
{
    /// <summary>
    /// Returns all repository access entries for the given user.
    /// </summary>
    /// <param name="userId">The primary key of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<UserRepositoryAccess>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all repository access entries for the given repository configuration.
    /// </summary>
    /// <param name="repositoryConfigurationId">The primary key of the repository configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<UserRepositoryAccess>> GetByRepositoryConfigurationIdAsync(Guid repositoryConfigurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the given user has access to the given repository configuration.
    /// </summary>
    /// <param name="userId">The primary key of the user.</param>
    /// <param name="repositoryConfigurationId">The primary key of the repository configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> HasAccessAsync(Guid userId, Guid repositoryConfigurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new user repository access entry.
    /// </summary>
    /// <param name="access">The access entry to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(UserRepositoryAccess access, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the user repository access entry with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the access entry to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
