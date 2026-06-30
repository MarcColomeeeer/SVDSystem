using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Manages file type filters that control which file extensions are analyzed.
/// </summary>
[Route("api/file-type-filters")]
[Authorize]
public class FileTypeFiltersController : SvdControllerBase
{
    private readonly IFileTypeFilterRepository _repo;
    private readonly IRepositoryConfigurationRepository _repoConfig;
    private readonly IFilterGroupRepository _filterGroupRepo;

    public FileTypeFiltersController(
        IFileTypeFilterRepository repo,
        IRepositoryConfigurationRepository repoConfig,
        IFilterGroupRepository filterGroupRepo,
        IUserRepository userRepo)
        : base(userRepo)
    {
        _repo = repo;
        _repoConfig = repoConfig;
        _filterGroupRepo = filterGroupRepo;
    }

    /// <summary>
    /// Returns all file type filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<FileTypeFilterDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok((await _repo.GetAllAsync(cancellationToken)).Select(ToDto));

    /// <summary>
    /// Returns a single file type filter by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FileTypeFilterDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var filter = await _repo.GetByIdAsync(id, cancellationToken);
        return filter is null ? NotFound() : Ok(ToDto(filter));
    }

    /// <summary>
    /// Creates a new file type filter.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<FileTypeFilterDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UpsertFileTypeFilterDto dto, CancellationToken cancellationToken)
    {
        var name = dto.Name.Trim();
        var extension = dto.Extension.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(extension)) return BadRequest("Extension is required.");
        if (await _repo.ExistsByNameAsync(name, null, cancellationToken))
            return Conflict($"A file type named \"{name}\" already exists.");

        var filter = new FileTypeFilter { Name = name, Extension = extension, CreatedBy = await ResolveUserAsync(cancellationToken) };
        await _repo.AddAsync(filter, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = filter.Id }, ToDto(filter));
    }

    /// <summary>
    /// Updates an existing file type filter.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<FileTypeFilterDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertFileTypeFilterDto dto, CancellationToken cancellationToken)
    {
        var filter = await _repo.GetByIdAsync(id, cancellationToken);
        if (filter is null) return NotFound();
        if (!CanEdit(filter)) return Forbid();

        var name = dto.Name.Trim();
        var extension = dto.Extension.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(extension)) return BadRequest("Extension is required.");
        if (await _repo.ExistsByNameAsync(name, id, cancellationToken))
            return Conflict($"A file type named \"{name}\" already exists.");

        filter.Name = name;
        filter.Extension = extension;
        await _repo.UpdateAsync(filter, cancellationToken);
        return Ok(ToDto(filter));
    }

    /// <summary>
    /// Deletes a file type filter. Blocked if the extension is referenced by any repository configuration or filter group.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var filter = await _repo.GetByIdAsync(id, cancellationToken);
        if (filter is null) return NotFound();
        if (!CanEdit(filter)) return Forbid();

        var allRepos = await _repoConfig.GetAllAsync(cancellationToken);
        if (allRepos.Any(r => r.FileTypeFilters
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(filter.Extension, StringComparer.OrdinalIgnoreCase)))
            return Conflict($"Cannot delete \"{filter.Name}\" because it is used in one or more repository configurations.");

        var allGroups = await _filterGroupRepo.GetAllAsync(cancellationToken);
        if (allGroups.Any(g => g.FileTypeExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(filter.Extension, StringComparer.OrdinalIgnoreCase)))
            return Conflict($"Cannot delete \"{filter.Name}\" because it is used in one or more filter groups.");

        await _repo.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // -- Helpers --------------------------------------------------------------

    private bool CanEdit(FileTypeFilter f) =>
        User.IsInRole("Admin") || f.CreatedBy?.ObjectId == GetObjectId();

    private static FileTypeFilterDto ToDto(FileTypeFilter f) =>
        new(f.Id, f.Name, f.Extension, f.CreatedBy?.ObjectId ?? string.Empty, f.CreatedBy?.DisplayName ?? string.Empty);
}
