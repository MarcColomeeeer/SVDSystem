using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="FileTypeFilter"/> records.
/// </summary>
public interface IFileTypeFilterRepository
{
    /// <summary>
    /// Returns all file type filters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<FileTypeFilter>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the file type filter for the given primary key, or null if not found.
    /// </summary>
    /// <param name="id">The primary key of the file type filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<FileTypeFilter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a file type filter with the given name already exists,
    /// optionally excluding a record by ID (used during updates).
    /// </summary>
    /// <param name="name">The name to check for uniqueness.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new file type filter.
    /// </summary>
    /// <param name="filter">The filter to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(FileTypeFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing file type filter.
    /// </summary>
    /// <param name="filter">The filter with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(FileTypeFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the file type filter with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the filter to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
