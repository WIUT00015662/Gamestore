using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gamestore.BLL.Services;

public class SmtpEmailService(
    IOptions<SmtpSettings> smtpSettings,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = smtpSettings.Value;
    private readonly ILogger<SmtpEmailService> _logger = logger;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await SendEmailsAsync([to], subject, body);
    }

    public async Task SendEmailsAsync(IEnumerable<string> recipients, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("SMTP settings not configured. Email not sent to recipients.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));

            foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                message.To.Add(MailboxAddress.Parse(recipient));
            }

            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSSL);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {RecipientCount} recipients with subject '{Subject}'", message.To.Count, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with subject '{Subject}'", subject);
            throw;
        }
    }
}
