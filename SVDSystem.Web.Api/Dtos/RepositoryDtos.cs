using SVDSystem.Domain.Entities.Vulnerability;

namespace SVDSystem.Web.Api.Dtos;

public record RepositoryDto(
    Guid Id,
    string RepositoryId,
    string RepositoryName,
    string ProjectName,
    string RemoteUrl,
    bool Enabled,
    string? CustomPrompt,
    bool UseCategories,
    VulnerabilityLevel SeverityThreshold,
    string VulnerabilityCategories,
    string IgnorePaths,
    string FileTypeFilters,
    bool IncludeAddedFiles,
    bool IncludeDeletedFiles,
    bool IncludeModifiedFiles);

public record UpdateRepositoryDto(
    bool Enabled,
    string? CustomPrompt,
    bool UseCategories,
    VulnerabilityLevel SeverityThreshold,
    string VulnerabilityCategories,
    string IgnorePaths,
    string FileTypeFilters,
    bool IncludeAddedFiles,
    bool IncludeDeletedFiles,
    bool IncludeModifiedFiles);
