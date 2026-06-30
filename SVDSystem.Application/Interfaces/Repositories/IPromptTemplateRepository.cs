using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Application.Interfaces;

/// <summary>
/// Repository for persisting and querying <see cref="PromptTemplate"/> records.
/// </summary>
public interface IPromptTemplateRepository
{
    /// <summary>
    /// Returns all prompt templates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the prompt template for the given primary key, or null if not found.
    /// </summary>
    /// <param name="id">The primary key of the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new prompt template.
    /// </summary>
    /// <param name="template">The template to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing prompt template.
    /// </summary>
    /// <param name="template">The template with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the prompt template with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the template to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
