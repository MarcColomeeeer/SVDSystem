using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICategoryGroupRepository"/>.
/// </summary>
public class CategoryGroupRepository : ICategoryGroupRepository
{
    private readonly AppDbContext _db;
    public CategoryGroupRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryGroup>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.CategoryGroups.Include(c => c.CreatedBy).OrderBy(c => c.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<CategoryGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.CategoryGroups.Include(c => c.CreatedBy).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, Guid? excludeId, CancellationToken cancellationToken = default) =>
        _db.CategoryGroups.AnyAsync(g =>
            g.Name.ToLower() == name.ToLower() &&
            EF.Property<Guid>(g, "created_by_id") == ownerId &&
            (excludeId == null || g.Id != excludeId.Value),
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(CategoryGroup group, CancellationToken cancellationToken = default)
    {
        _db.CategoryGroups.Add(group);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(CategoryGroup group, CancellationToken cancellationToken = default)
    {
        _db.CategoryGroups.Update(group);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CategoryGroups.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.CategoryGroups.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
