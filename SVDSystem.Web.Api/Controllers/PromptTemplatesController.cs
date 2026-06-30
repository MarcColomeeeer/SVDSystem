using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;
using SVDSystem.Web.Api.Dtos;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Manages reusable prompt templates for the Ollama analysis pipeline.
/// </summary>
[Route("api/prompts")]
[Authorize]
public class PromptTemplatesController : SvdControllerBase
{
    private readonly IPromptTemplateRepository _repo;

    public PromptTemplatesController(IPromptTemplateRepository repo, IUserRepository userRepo)
        : base(userRepo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Returns all prompt templates.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<PromptTemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok((await _repo.GetAllAsync(cancellationToken)).Select(ToDto));

    /// <summary>
    /// Returns a single prompt template by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PromptTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var template = await _repo.GetByIdAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(ToDto(template));
    }

    /// <summary>
    /// Creates a new prompt template owned by the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<PromptTemplateDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UpsertPromptTemplateDto dto, CancellationToken cancellationToken)
    {
        var template = new PromptTemplate
        {
            Name = dto.Name,
            Content = dto.Content,
            CreatedBy = await ResolveUserAsync(cancellationToken),
            IsSystem = false
        };
        await _repo.AddAsync(template, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, ToDto(template));
    }

    /// <summary>
    /// Updates an existing prompt template. System templates can only be edited by admins.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PromptTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPromptTemplateDto dto, CancellationToken cancellationToken)
    {
        var template = await _repo.GetByIdAsync(id, cancellationToken);
        if (template is null) return NotFound();
        if (!CanEdit(template)) return Forbid();

        template.Name = dto.Name;
        template.Content = dto.Content;
        await _repo.UpdateAsync(template, cancellationToken);
        return Ok(ToDto(template));
    }

    /// <summary>
    /// Deletes a prompt template. System templates can only be deleted by admins.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var template = await _repo.GetByIdAsync(id, cancellationToken);
        if (template is null) return NotFound();
        if (!CanEdit(template)) return Forbid();

        await _repo.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // -- Helpers --------------------------------------------------------------

    private bool CanEdit(PromptTemplate t) =>
        User.IsInRole("Admin") || (!t.IsSystem && t.CreatedBy?.ObjectId == GetObjectId());

    private static PromptTemplateDto ToDto(PromptTemplate t) =>
        new(t.Id, t.Name, t.Content, t.CreatedBy?.ObjectId ?? string.Empty, t.CreatedBy?.DisplayName ?? string.Empty, t.IsSystem);
}
