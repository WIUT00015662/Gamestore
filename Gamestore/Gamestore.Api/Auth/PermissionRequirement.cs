using Microsoft.AspNetCore.Authorization;

namespace Gamestore.Api.Auth;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
