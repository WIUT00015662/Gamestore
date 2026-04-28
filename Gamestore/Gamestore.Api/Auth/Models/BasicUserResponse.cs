namespace Gamestore.Api.Auth.Models;

public class BasicUserResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Email { get; set; }
}
