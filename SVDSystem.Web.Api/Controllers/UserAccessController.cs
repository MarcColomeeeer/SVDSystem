using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Admin-only endpoints for managing which users have access to which repository configurations.
/// </summary>
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UserAccessController : SvdControllerBase
{
    private readonly IUserRepositoryAccessRepository _accessRepo;
    private readonly IRepositoryConfigurationRepository _repoConfigRepo;

    public UserAccessController(
        IUserRepositoryAccessRepository accessRepo,
        IRepositoryConfigurationRepository repoConfigRepo,
        IUserRepository userRepo)
        : base(userRepo)
    {
        _accessRepo = accessRepo;
        _repoConfigRepo = repoConfigRepo;
    }

    /// <summary>
    /// Returns all known users (excluding the internal system user).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await GetUserRepository().GetAllAsync(cancellationToken);
        return Ok(users
            .Where(u => u.Id != new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"))
            .Select(u => new UserDto(u.Id, u.DisplayName, u.Email)));
    }

    /// <summary>
    /// Returns all repository accesses granted to a specific user.
    /// </summary>
    [HttpGet("{userId:guid}/repositories")]
    [ProducesResponseType<IEnumerable<UserAccessDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAccesses(Guid userId, CancellationToken cancellationToken)
    {
        var accesses = await _accessRepo.GetByUserIdAsync(userId, cancellationToken);
        return Ok(accesses.Select(ToDto));
    }

    /// <summary>
    /// Returns all users that have access to a specific repository configuration.
    /// </summary>
    [HttpGet("repository/{repositoryConfigurationId:guid}")]
    [ProducesResponseType<IEnumerable<UserAccessDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepositoryAccesses(Guid repositoryConfigurationId, CancellationToken cancellationToken)
    {
        var accesses = await _accessRepo.GetByRepositoryConfigurationIdAsync(repositoryConfigurationId, cancellationToken);
        return Ok(accesses.Select(ToDto));
    }

    /// <summary>
    /// Grants a user access to a repository configuration.
    /// </summary>
    [HttpPost("{userId:guid}/repositories")]
    [ProducesResponseType<UserAccessDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GrantAccess(Guid userId, [FromBody] GrantAccessDto dto, CancellationToken cancellationToken)
    {
        var userRepo = GetUserRepository();
        var all = await userRepo.GetAllAsync(cancellationToken);
        var user = all.FirstOrDefault(u => u.Id == userId);
        if (user is null) return NotFound("User not found.");

        if (await _accessRepo.HasAccessAsync(user.Id, dto.RepositoryConfigurationId, cancellationToken))
            return Conflict("User already has access to this repository.");

        var config = await _repoConfigRepo.GetByIdAsync(dto.RepositoryConfigurationId, cancellationToken);
        if (config is null) return NotFound("Repository configuration not found.");

        var access = new UserRepositoryAccess { User = user, RepositoryConfiguration = config };
        await _accessRepo.AddAsync(access, cancellationToken);
        return CreatedAtAction(nameof(GetUserAccesses), new { userId = user.Id }, ToDto(access));
    }

    /// <summary>
    /// Revokes a specific repository access record by its ID.
    /// </summary>
    [HttpDelete("access/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeAccess(Guid id, CancellationToken cancellationToken)
    {
        await _accessRepo.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // -- Helpers --------------------------------------------------------------

    // Expose the injected IUserRepository to the base without re-injecting it.
    private IUserRepository GetUserRepository() => HttpContext.RequestServices.GetRequiredService<IUserRepository>();

    private static UserAccessDto ToDto(UserRepositoryAccess a) =>
        new(
            a.Id,
            a.User?.Id ?? Guid.Empty,
            a.RepositoryConfiguration?.Id ?? Guid.Empty,
            a.RepositoryConfiguration?.ProjectName ?? string.Empty,
            a.RepositoryConfiguration?.RepositoryName ?? string.Empty,
            a.User?.DisplayName ?? string.Empty,
            a.User?.Email ?? string.Empty
        );
}
