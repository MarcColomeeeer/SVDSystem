using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="CategoryGroup"/> records.
/// </summary>
public interface ICategoryGroupRepository
{
    /// <summary>
    /// Returns all category groups.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<CategoryGroup>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the category group for the given primary key, or null if not found.
    /// </summary>
    /// <param name="id">The primary key of the category group.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CategoryGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a category group with the given name already exists for the specified owner,
    /// optionally excluding a record by ID (used during updates).
    /// </summary>
    /// <param name="name">The name to check for uniqueness.</param>
    /// <param name="ownerId">The primary key of the owner user.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, Guid? excludeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new category group.
    /// </summary>
    /// <param name="group">The group to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(CategoryGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category group.
    /// </summary>
    /// <param name="group">The group with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(CategoryGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the category group with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the group to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
