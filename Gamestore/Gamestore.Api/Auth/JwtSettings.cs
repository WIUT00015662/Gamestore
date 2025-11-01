namespace Gamestore.Api.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = "Gamestore";

    public string Audience { get; set; } = "GamestoreClient";

    public string Key { get; set; } = "super_secret_key_change_me_1234567890";

    public int ExpirationMinutes { get; set; } = 120;
}
