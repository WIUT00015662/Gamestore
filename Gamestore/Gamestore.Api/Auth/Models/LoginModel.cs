namespace Gamestore.Api.Auth.Models;

public class LoginModel
{
    public required string Login { get; set; }

    public required string Password { get; set; }

    public bool InternalAuth { get; set; }
}
