using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Manages filter groups that bundle ignore-path and file-type rules.
/// </summary>
[Route("api/filter-groups")]
[Authorize]
public class FilterGroupsController : SvdControllerBase
{
    private readonly IFilterGroupRepository _repo;

    public FilterGroupsController(IFilterGroupRepository repo, IUserRepository userRepo)
        : base(userRepo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Returns all filter groups.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<FilterGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok((await _repo.GetAllAsync(cancellationToken)).Select(ToDto));

    /// <summary>
    /// Returns a single filter group by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FilterGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await _repo.GetByIdAsync(id, cancellationToken);
        return group is null ? NotFound() : Ok(ToDto(group));
    }

    /// <summary>
    /// Creates a new filter group owned by the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<FilterGroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UpsertFilterGroupDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var currentUser = await ResolveUserAsync(cancellationToken);
        if (await _repo.ExistsByNameAndOwnerAsync(name, currentUser.Id, null, cancellationToken))
            return Conflict($"You already have a filter group named \"{name}\".");

        var group = new FilterGroup { Name = name, IgnorePaths = dto.IgnorePaths, FileTypeExtensions = dto.FileTypeExtensions, CreatedBy = currentUser };
        await _repo.AddAsync(group, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, ToDto(group));
    }

    /// <summary>
    /// Updates an existing filter group.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<FilterGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertFilterGroupDto dto, CancellationToken cancellationToken)
    {
        var group = await _repo.GetByIdAsync(id, cancellationToken);
        if (group is null) return NotFound();
        if (!CanEdit(group)) return Forbid();

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        if (await _repo.ExistsByNameAndOwnerAsync(name, group.CreatedBy!.Id, id, cancellationToken))
            return Conflict($"You already have a filter group named \"{name}\".");

        group.Name = name;
        group.IgnorePaths = dto.IgnorePaths;
        group.FileTypeExtensions = dto.FileTypeExtensions;
        await _repo.UpdateAsync(group, cancellationToken);
        return Ok(ToDto(group));
    }

    /// <summary>
    /// Deletes a filter group.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var group = await _repo.GetByIdAsync(id, cancellationToken);
        if (group is null) return NotFound();
        if (!CanEdit(group)) return Forbid();

        await _repo.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // -- Helpers --------------------------------------------------------------

    private bool CanEdit(FilterGroup g) =>
        User.IsInRole("Admin") || g.CreatedBy?.ObjectId == GetObjectId();

    private static FilterGroupDto ToDto(FilterGroup g) =>
        new(g.Id, g.Name, g.IgnorePaths, g.FileTypeExtensions, g.CreatedBy?.ObjectId ?? string.Empty, g.CreatedBy?.DisplayName ?? string.Empty);
}
