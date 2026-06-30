using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRepositoryConfigurationRepository"/>.
/// </summary>
public class RepositoryConfigurationRepository : IRepositoryConfigurationRepository
{
    private readonly AppDbContext _db;

    public RepositoryConfigurationRepository(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<RepositoryConfiguration> WithIncludes() =>
        _db.RepositoryConfigurations;

    /// <inheritdoc />
    public Task<RepositoryConfiguration?> GetByRepositoryIdAsync(
        string repositoryId,
        CancellationToken cancellationToken = default) =>
        WithIncludes()
           .FirstOrDefaultAsync(r => r.RepositoryId == repositoryId, cancellationToken);

    /// <inheritdoc />
    public Task<RepositoryConfiguration?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        WithIncludes()
           .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(RepositoryConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _db.RepositoryConfigurations.Add(configuration);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RepositoryConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _db.RepositoryConfigurations.Update(configuration);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RepositoryConfiguration>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await WithIncludes()
                 .OrderBy(r => r.ProjectName)
                 .ThenBy(r => r.RepositoryName)
                 .ToListAsync(cancellationToken);
}
