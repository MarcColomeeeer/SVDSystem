using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPromptTemplateRepository"/>.
/// </summary>
public class PromptTemplateRepository : IPromptTemplateRepository
{
    private readonly AppDbContext _db;
    public PromptTemplateRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.PromptTemplates.Include(p => p.CreatedBy).OrderBy(p => p.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.PromptTemplates.Include(p => p.CreatedBy).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        _db.PromptTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        _db.PromptTemplates.Update(template);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PromptTemplates.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.PromptTemplates.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
