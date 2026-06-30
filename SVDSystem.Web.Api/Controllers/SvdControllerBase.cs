using Microsoft.AspNetCore.Mvc;
using SVDSystem.Application.Interfaces;
using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Web.Api.Controllers;

/// <summary>
/// Base controller that provides shared user-resolution helpers for all SVD controllers.
/// </summary>
[ApiController]
public abstract class SvdControllerBase : ControllerBase
{
    private readonly IUserRepository _userRepo;

    protected SvdControllerBase(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// Returns the Azure AD object identifier of the currently authenticated user.
    /// </summary>
    protected string GetObjectId() =>
        User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
        ?? User.FindFirst("oid")?.Value
        ?? string.Empty;

    /// <summary>
    /// Resolves the current user from the database, creating a new record on first login.
    /// </summary>
    protected async Task<User> ResolveUserAsync(CancellationToken cancellationToken)
    {
        var objectId = GetObjectId();
        var user = await _userRepo.GetByObjectIdAsync(objectId, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                ObjectId  = objectId,
                DisplayName = User.FindFirst("name")?.Value
                           ?? User.FindFirst("preferred_username")?.Value
                           ?? string.Empty,
                Email = User.FindFirst("email")?.Value
                      ?? User.FindFirst("preferred_username")?.Value
                      ?? string.Empty,
            };
            await _userRepo.AddAsync(user, cancellationToken);
        }
        return user;
    }
}
