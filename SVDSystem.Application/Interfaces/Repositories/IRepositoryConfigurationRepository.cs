using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="RepositoryConfiguration"/> records.
/// </summary>
public interface IRepositoryConfigurationRepository
{
    /// <summary>
    /// Returns the configuration for the given Azure DevOps repository ID, or null if it has never been seen before.
    /// </summary>
    /// <param name="repositoryId">The Azure DevOps repository ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<RepositoryConfiguration?> GetByRepositoryIdAsync(string repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the configuration for the given internal primary key, or null if not found.
    /// </summary>
    /// <param name="id">The internal primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<RepositoryConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all known repository configurations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<RepositoryConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new configuration record (first time a repository is seen).
    /// </summary>
    /// <param name="configuration">The configuration to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(RepositoryConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configuration record.
    /// </summary>
    /// <param name="configuration">The configuration with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(RepositoryConfiguration configuration, CancellationToken cancellationToken = default);
}
