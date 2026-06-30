using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Manages category groups used to organise vulnerability categories.
/// </summary>
[Route("api/category-groups")]
[Authorize]
public class CategoryGroupsController : SvdControllerBase
{
    private readonly ICategoryGroupRepository _repo;

    public CategoryGroupsController(ICategoryGroupRepository repo, IUserRepository userRepo)
        : base(userRepo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Returns all category groups.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CategoryGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok((await _repo.GetAllAsync(cancellationToken)).Select(ToDto));

    /// <summary>
    /// Returns a single category group by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await _repo.GetByIdAsync(id, cancellationToken);
        return group is null ? NotFound() : Ok(ToDto(group));
    }

    /// <summary>
    /// Creates a new category group owned by the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CategoryGroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UpsertCategoryGroupDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var currentUser = await ResolveUserAsync(cancellationToken);
        if (await _repo.ExistsByNameAndOwnerAsync(name, currentUser.Id, null, cancellationToken))
            return Conflict($"You already have a category group named \"{name}\".");

        var group = new CategoryGroup { Name = name, Categories = dto.Categories, CreatedBy = currentUser };
        await _repo.AddAsync(group, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, ToDto(group));
    }

    /// <summary>
    /// Updates an existing category group.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertCategoryGroupDto dto, CancellationToken cancellationToken)
    {
        var group = await _repo.GetByIdAsync(id, cancellationToken);
        if (group is null) return NotFound();
        if (!CanEdit(group)) return Forbid();

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        if (await _repo.ExistsByNameAndOwnerAsync(name, group.CreatedBy!.Id, id, cancellationToken))
            return Conflict($"You already have a category group named \"{name}\".");

        group.Name = name;
        group.Categories = dto.Categories;
        await _repo.UpdateAsync(group, cancellationToken);
        return Ok(ToDto(group));
    }

    /// <summary>
    /// Deletes a category group.
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

    private bool CanEdit(CategoryGroup g) =>
        User.IsInRole("Admin") || g.CreatedBy?.ObjectId == GetObjectId();

    private static CategoryGroupDto ToDto(CategoryGroup g) =>
        new(g.Id, g.Name, g.Categories, g.CreatedBy?.ObjectId ?? string.Empty, g.CreatedBy?.DisplayName ?? string.Empty);
}
