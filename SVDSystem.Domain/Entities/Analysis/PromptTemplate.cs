namespace SVDSystem.Domain.Entities.Analysis;

/// <summary>
/// A reusable custom prompt template created by users.
/// </summary>
public class PromptTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name shown in the frontend selector.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The additional instructions text. For system prompts this is the full prompt.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The user who created this template.
    /// </summary>
    public User? CreatedBy { get; set; }

    /// <summary>
    /// True for the two built-in system prompts. Only admins may edit them.
    /// </summary>
    public bool IsSystem { get; set; } = false;
}
