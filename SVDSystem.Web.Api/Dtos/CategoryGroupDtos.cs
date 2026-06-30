namespace SVDSystem.Web.Api.Dtos;

public record CategoryGroupDto(
    Guid Id,
    string Name,
    string Categories,
    string CreatedByObjectId,
    string CreatedByDisplayName);

public record UpsertCategoryGroupDto(string Name, string Categories);

public record SaveAsGroupDto(string Name);
