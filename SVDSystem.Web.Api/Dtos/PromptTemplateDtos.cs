namespace SVDSystem.Web.Api.Dtos;

public record PromptTemplateDto(
    Guid Id,
    string Name,
    string Content,
    string CreatedByObjectId,
    string CreatedByDisplayName,
    bool IsSystem);

public record UpsertPromptTemplateDto(string Name, string Content);

public record SavePromptAsTemplateDto(string Name);
