namespace Gamestore.Api.Auth.Models;

public class CreateOrUpdateUserRequest
{
    public required UserBody User { get; set; }

    public required List<Guid> Roles { get; set; }

    public required string Password { get; set; }
}
