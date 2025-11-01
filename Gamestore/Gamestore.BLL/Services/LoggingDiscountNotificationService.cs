using Gamestore.BLL.DTOs.Deals;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public class LoggingDiscountNotificationService(ILogger<LoggingDiscountNotificationService> logger) : IDiscountNotificationService
{
    private readonly ILogger<LoggingDiscountNotificationService> _logger = logger;

    public Task NotifyUsersAsync(IEnumerable<string> recipients, IEnumerable<DiscountedGameResponse> discountedGames)
    {
        List<DiscountedGameResponse> deals = [.. discountedGames];
        if (deals.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var recipient in recipients)
        {
            _logger.LogInformation(
                "Discount email to {Recipient}. Deals: {Deals}",
                recipient,
                string.Join(" | ", deals.Select(d => $"{d.GameName} ({d.DiscountPercent:0.##}% off at {d.Vendor})")));
        }

        return Task.CompletedTask;
    }
}
