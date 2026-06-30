using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Infrastructure.Persistence;

namespace SVDSystem.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepositoryAccessRepository"/>.
/// </summary>
public class UserRepositoryAccessRepository : IUserRepositoryAccessRepository
{
    private readonly AppDbContext _db;
    public UserRepositoryAccessRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserRepositoryAccess>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.UserRepositoryAccesses
                 .Include(a => a.RepositoryConfiguration)
                 .Include(a => a.User)
                 .Where(a => EF.Property<Guid>(a, "user_id") == userId)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserRepositoryAccess>> GetByRepositoryConfigurationIdAsync(Guid repositoryConfigurationId, CancellationToken cancellationToken = default) =>
        await _db.UserRepositoryAccesses
                 .Include(a => a.User)
                 .Where(a => EF.Property<Guid>(a, "repository_configuration_id") == repositoryConfigurationId)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> HasAccessAsync(Guid userId, Guid repositoryConfigurationId, CancellationToken cancellationToken = default) =>
        _db.UserRepositoryAccesses.AnyAsync(
            a => EF.Property<Guid>(a, "user_id") == userId &&
                 EF.Property<Guid>(a, "repository_configuration_id") == repositoryConfigurationId,
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(UserRepositoryAccess access, CancellationToken cancellationToken = default)
    {
        _db.UserRepositoryAccesses.Add(access);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.UserRepositoryAccesses.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _db.UserRepositoryAccesses.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
