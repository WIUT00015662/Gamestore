using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gamestore.Api.Services;

/// <summary>
/// Implementation of ICurrentUserService that extracts user information from HTTP context.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)
            ? throw new ArgumentException("User ID not found in JWT claims.")
            : userId;
    }

    public string GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
            ?? throw new ArgumentException("User name not found in JWT claims.");
    }

    public bool HasPermission(string permission)
    {
        return _httpContextAccessor.HttpContext?.User.HasClaim("permission", permission) ?? false;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
