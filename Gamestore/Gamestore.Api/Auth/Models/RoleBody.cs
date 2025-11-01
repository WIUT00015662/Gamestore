namespace Gamestore.Api.Auth.Models;

public class RoleBody
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}
