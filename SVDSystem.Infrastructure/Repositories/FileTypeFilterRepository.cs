using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IFileTypeFilterRepository"/>.
/// </summary>
public class FileTypeFilterRepository : IFileTypeFilterRepository
{
    private readonly AppDbContext _db;
    public FileTypeFilterRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<FileTypeFilter>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.FileTypeFilters.Include(f => f.CreatedBy).OrderBy(f => f.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<FileTypeFilter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FileTypeFilters.Include(f => f.CreatedBy).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default) =>
        _db.FileTypeFilters.AnyAsync(
            f => f.Name.ToLower() == name.ToLower() && (excludeId == null || f.Id != excludeId),
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(FileTypeFilter filter, CancellationToken cancellationToken = default)
    {
        _db.FileTypeFilters.Add(filter);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(FileTypeFilter filter, CancellationToken cancellationToken = default)
    {
        _db.FileTypeFilters.Update(filter);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.FileTypeFilters.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.FileTypeFilters.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
