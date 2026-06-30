using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="User"/> records.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Returns the user with the given Entra Object ID, or null if not found.
    /// </summary>
    /// <param name="objectId">The Entra ID object ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<User?> GetByObjectIdAsync(string objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new user.
    /// </summary>
    /// <param name="user">The user to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user's cached display name and email.
    /// </summary>
    /// <param name="user">The user with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
