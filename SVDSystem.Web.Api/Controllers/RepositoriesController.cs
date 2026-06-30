using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Manages repository analysis configurations.
/// Admins see all repositories; regular users see only their assigned ones.
/// </summary>
[Route("api/repositories")]
[Authorize]
public class RepositoriesController : SvdControllerBase
{
    private readonly IRepositoryConfigurationRepository _repo;
    private readonly IUserRepositoryAccessRepository _accessRepo;
    private readonly IPromptTemplateRepository _promptRepo;
    private readonly ICategoryGroupRepository _categoryGroupRepo;

    public RepositoriesController(
        IRepositoryConfigurationRepository repo,
        IUserRepositoryAccessRepository accessRepo,
        IPromptTemplateRepository promptRepo,
        ICategoryGroupRepository categoryGroupRepo,
        IUserRepository userRepo)
        : base(userRepo)
    {
        _repo = repo;
        _accessRepo = accessRepo;
        _promptRepo = promptRepo;
        _categoryGroupRepo = categoryGroupRepo;
    }

    /// <summary>
    /// Returns all repositories the caller is allowed to see.
    /// Admins receive the full list; regular users receive only their assigned repositories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<RepositoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
            return Ok((await _repo.GetAllAsync(cancellationToken)).Select(ToDto));

        var currentUser = await ResolveUserAsync(cancellationToken);
        var accesses = await _accessRepo.GetByUserIdAsync(currentUser.Id, cancellationToken);
        return Ok(accesses.Select(a => ToDto(a.RepositoryConfiguration!)));
    }

    /// <summary>
    /// Returns a single repository configuration by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RepositoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var config = await _repo.GetByIdAsync(id, cancellationToken);
        if (config is null) return NotFound();
        if (!await CanAccessAsync(config.Id, cancellationToken)) return Forbid();
        return Ok(ToDto(config));
    }

    /// <summary>
    /// Updates the analysis configuration for a repository.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<RepositoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRepositoryDto dto, CancellationToken cancellationToken)
    {
        var config = await _repo.GetByIdAsync(id, cancellationToken);
        if (config is null) return NotFound();
        if (!await CanAccessAsync(config.Id, cancellationToken)) return Forbid();

        config.Enabled = dto.Enabled;
        config.CustomPrompt = dto.CustomPrompt;
        config.UseCategories = dto.UseCategories;
        config.SeverityThreshold = dto.SeverityThreshold;
        config.VulnerabilityCategories = dto.VulnerabilityCategories;
        config.IgnorePaths = dto.IgnorePaths;
        config.FileTypeFilters = dto.FileTypeFilters;
        config.IncludeAddedFiles = dto.IncludeAddedFiles;
        config.IncludeDeletedFiles = dto.IncludeDeletedFiles;
        config.IncludeModifiedFiles = dto.IncludeModifiedFiles;

        await _repo.UpdateAsync(config, cancellationToken);
        return Ok(ToDto(config));
    }

    /// <summary>
    /// Saves the current vulnerability categories of a repository as a reusable category group.
    /// </summary>
    [HttpPost("{id:guid}/save-categories-as-group")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveCategoriesAsGroup(Guid id, [FromBody] SaveAsGroupDto dto, CancellationToken cancellationToken)
    {
        var config = await _repo.GetByIdAsync(id, cancellationToken);
        if (config is null) return NotFound();
        if (!await CanAccessAsync(config.Id, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(config.VulnerabilityCategories))
            return BadRequest("No vulnerability categories to save.");

        var group = new CategoryGroup
        {
            Name = dto.Name,
            Categories = config.VulnerabilityCategories,
            CreatedBy = await ResolveUserAsync(cancellationToken),
        };
        await _categoryGroupRepo.AddAsync(group, cancellationToken);
        return Ok(new { group.Id, group.Name });
    }

    /// <summary>
    /// Saves the current custom prompt of a repository as a reusable prompt template.
    /// </summary>
    [HttpPost("{id:guid}/save-prompt-as-template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SavePromptAsTemplate(Guid id, [FromBody] SavePromptAsTemplateDto dto, CancellationToken cancellationToken)
    {
        var config = await _repo.GetByIdAsync(id, cancellationToken);
        if (config is null) return NotFound();
        if (!await CanAccessAsync(config.Id, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(config.CustomPrompt))
            return BadRequest("No custom prompt to save.");

        var template = new PromptTemplate
        {
            Name = dto.Name,
            Content = config.CustomPrompt,
            CreatedBy = await ResolveUserAsync(cancellationToken),
            IsSystem = false
        };
        await _promptRepo.AddAsync(template, cancellationToken);
        return Ok(new { template.Id, template.Name });
    }

    // -- Helpers --------------------------------------------------------------

    private async Task<bool> CanAccessAsync(Guid repositoryConfigurationId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin")) return true;
        var currentUser = await ResolveUserAsync(cancellationToken);
        return await _accessRepo.HasAccessAsync(currentUser.Id, repositoryConfigurationId, cancellationToken);
    }

    private static RepositoryDto ToDto(RepositoryConfiguration c) => new(
        c.Id, c.RepositoryId, c.RepositoryName, c.ProjectName, c.RemoteUrl,
        c.Enabled, c.CustomPrompt, c.UseCategories, c.SeverityThreshold,
        c.VulnerabilityCategories, c.IgnorePaths, c.FileTypeFilters,
        c.IncludeAddedFiles, c.IncludeDeletedFiles, c.IncludeModifiedFiles);
}
