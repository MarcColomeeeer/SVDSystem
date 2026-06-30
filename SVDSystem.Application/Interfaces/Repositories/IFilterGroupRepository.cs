using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="FilterGroup"/> records.
/// </summary>
public interface IFilterGroupRepository
{
    /// <summary>
    /// Returns all filter groups.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<FilterGroup>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the filter group for the given primary key, or null if not found.
    /// </summary>
    /// <param name="id">The primary key of the filter group.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<FilterGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a filter group with the given name already exists,
    /// optionally excluding a record by ID (used during updates).
    /// </summary>
    /// <param name="name">The name to check for uniqueness.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a filter group with the given name already exists for the specified owner,
    /// optionally excluding a record by ID (used during updates).
    /// </summary>
    /// <param name="name">The name to check for uniqueness.</param>
    /// <param name="ownerId">The primary key of the owner user.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, Guid? excludeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new filter group.
    /// </summary>
    /// <param name="group">The group to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(FilterGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing filter group.
    /// </summary>
    /// <param name="group">The group with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(FilterGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the filter group with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the group to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
