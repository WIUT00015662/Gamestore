using Gamestore.BLL.DTOs.Deals;

namespace Gamestore.BLL.Services;

public interface IGameDealsService
{
    Task<DiscountPollingResultResponse> PollDiscountsAsync();

    Task<IReadOnlyCollection<DiscountedGameResponse>> GetLatestFeaturedDiscountsAsync(int take = 5);

    Task<IReadOnlyCollection<DiscountedGameResponse>> GetAllCurrentDiscountsAsync();

    Task<IReadOnlyCollection<GameVendorOfferResponse>> GetOffersByGameKeyAsync(string key);

    Task<EmailSubscriptionResponse> SubscribeEmailAsync(string email);

    Task UnsubscribeEmailAsync(string email);
}
