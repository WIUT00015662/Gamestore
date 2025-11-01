namespace Gamestore.Api.Auth.Models;

public class UserBody
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}
