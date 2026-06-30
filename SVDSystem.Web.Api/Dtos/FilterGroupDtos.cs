namespace SVDSystem.Web.Api.Dtos;

public record FilterGroupDto(
    Guid Id,
    string Name,
    string IgnorePaths,
    string FileTypeExtensions,
    string CreatedByObjectId,
    string CreatedByDisplayName);

public record UpsertFilterGroupDto(string Name, string IgnorePaths, string FileTypeExtensions);
