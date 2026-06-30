namespace SVDSystem.Web.Api.Dtos;

public record UserDto(Guid Id, string DisplayName, string Email);

/// <summary>
/// Represents a granted access record returned to the client.
/// Includes both the target repository configuration identity and display fields so the UI can show
/// repository/project names without additional API calls.
/// </summary>
public record UserAccessDto(
    Guid Id,
    Guid UserId,
    Guid RepositoryConfigurationId,
    string RepositoryProjectName,
    string RepositoryName,
    string DisplayName,
    string Email
);

public record GrantAccessDto(Guid RepositoryConfigurationId);
