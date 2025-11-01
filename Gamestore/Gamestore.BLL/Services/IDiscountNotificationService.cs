using Gamestore.BLL.DTOs.Deals;

namespace Gamestore.BLL.Services;

public interface IDiscountNotificationService
{
    Task NotifyUsersAsync(IEnumerable<string> recipients, IEnumerable<DiscountedGameResponse> discountedGames);
}
