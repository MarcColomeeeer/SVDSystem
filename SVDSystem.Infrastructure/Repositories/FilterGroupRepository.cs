using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IFilterGroupRepository"/>.
/// </summary>
public class FilterGroupRepository : IFilterGroupRepository
{
    private readonly AppDbContext _db;
    public FilterGroupRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<FilterGroup>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.FilterGroups.Include(f => f.CreatedBy).OrderBy(f => f.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<FilterGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FilterGroups.Include(f => f.CreatedBy).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default) =>
        _db.FilterGroups.AnyAsync(
            f => f.Name.ToLower() == name.ToLower() && (excludeId == null || f.Id != excludeId),
            cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, Guid? excludeId, CancellationToken cancellationToken = default) =>
        _db.FilterGroups.AnyAsync(
            f => f.Name.ToLower() == name.ToLower() &&
                 EF.Property<Guid>(f, "created_by_id") == ownerId &&
                 (excludeId == null || f.Id != excludeId),
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(FilterGroup group, CancellationToken cancellationToken = default)
    {
        _db.FilterGroups.Add(group);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(FilterGroup group, CancellationToken cancellationToken = default)
    {
        _db.FilterGroups.Update(group);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.FilterGroups.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.FilterGroups.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
