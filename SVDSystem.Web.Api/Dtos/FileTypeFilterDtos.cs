namespace SVDSystem.Web.Api.Dtos;

public record FileTypeFilterDto(
    Guid Id,
    string Name,
    string Extension,
    string CreatedByObjectId,
    string CreatedByDisplayName);

public record UpsertFileTypeFilterDto(string Name, string Extension);
