using Gamestore.BLL.DTOs.Deals;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public class EmailDiscountNotificationService(
    IEmailService emailService,
    ILogger<EmailDiscountNotificationService> logger) : IDiscountNotificationService
{
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<EmailDiscountNotificationService> _logger = logger;

    public async Task NotifyUsersAsync(IEnumerable<string> recipients, IEnumerable<DiscountedGameResponse> discountedGames)
    {
        var deals = discountedGames.ToList();
        if (deals.Count == 0)
        {
            return;
        }

        var subject = $"🎮 {deals.Count} Great Deals Found on GameStore!";
        var body = BuildEmailBody(deals);

        try
        {
            await _emailService.SendEmailsAsync(recipients, subject, body);
            _logger.LogInformation("Discount notification sent to {RecipientCount} users for {DealCount} deals",
                recipients.Count(), deals.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send discount notifications");
        }
    }

    private static string BuildEmailBody(List<DiscountedGameResponse> deals)
    {
        var dealsHtml = string.Join("\n", deals.Select(deal => $@"
                <tr>
                    <td style='padding: 15px; border-bottom: 1px solid #ddd;'>
                        <strong>{deal.GameName}</strong><br/>
                        <small>Vendor: {deal.Vendor}</small>
                    </td>
                    <td style='padding: 15px; border-bottom: 1px solid #ddd; text-align: right;'>
                        <span style='color: red; font-weight: bold;'>{deal.DiscountPercent:0.##}% OFF</span><br/>
                        <small><strike>${deal.OriginalPrice:0.00}</strike> → <strong>${deal.DiscountedPrice:0.00}</strong></small>
                    </td>
                    <td style='padding: 15px; border-bottom: 1px solid #ddd;'>
                        <a href='{deal.PurchaseUrl}' style='background-color: #007bff; color: white; padding: 8px 12px; text-decoration: none; border-radius: 4px; display: inline-block;'>Buy Now</a>
                    </td>
                </tr>"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .content {{ margin: 20px 0; }}
        table {{ width: 100%; border-collapse: collapse; }}
        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎮 GameStore Deals Alert</h1>
            <p>Check out these fantastic discounts!</p>
        </div>
        
        <div class='content'>
            <p>Hi there!</p>
            <p>We found <strong>{deals.Count} great deals</strong> for games you might like:</p>
            
            <table>
                <thead>
                    <tr style='background-color: #f8f9fa;'>
                        <th style='padding: 15px; text-align: left; border-bottom: 2px solid #007bff;'>Game</th>
                        <th style='padding: 15px; text-align: right; border-bottom: 2px solid #007bff;'>Discount</th>
                        <th style='padding: 15px; text-align: center; border-bottom: 2px solid #007bff;'>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {dealsHtml}
                </tbody>
            </table>
        </div>
        
        <div class='footer'>
            <p>Happy gaming! 🎉</p>
            <p><a href='https://gamestore.local'>Visit GameStore</a> | <a href='https://gamestore.local/preferences'>Manage Preferences</a></p>
        </div>
    </div>
</body>
</html>";
    }
}
