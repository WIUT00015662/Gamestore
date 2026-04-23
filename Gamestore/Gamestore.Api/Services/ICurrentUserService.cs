namespace Gamestore.Api.Services;

/// <summary>
/// Service for accessing current request's user information from JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID from JWT sub claim.
    /// </summary>
    Guid GetUserId();

    /// <summary>
    /// Gets the current user's name from JWT name claim.
    /// </summary>
    string GetUserName();

    /// <summary>
    /// Checks if current user has a specific permission.
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// Gets whether user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
