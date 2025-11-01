namespace Gamestore.Api.Auth.Models;

public class CreateOrUpdateRoleRequest
{
    public required RoleBody Role { get; set; }

    public required List<string> Permissions { get; set; }
}
