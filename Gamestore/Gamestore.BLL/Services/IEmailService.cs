namespace Gamestore.BLL.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);

    Task SendEmailsAsync(IEnumerable<string> recipients, string subject, string body);
}
