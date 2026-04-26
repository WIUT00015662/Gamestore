namespace Gamestore.BLL.Services;

public class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string? Host { get; set; }

    public int Port { get; set; } = 587;

    public bool UseSSL { get; set; } = true;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string FromEmail { get; set; } = "noreply@gamestore.local";

    public string FromName { get; set; } = "GameStore";
}
