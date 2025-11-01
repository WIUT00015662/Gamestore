using Gamestore.BLL.DTOs.Deals;

namespace Gamestore.BLL.Services;

public interface IGameDealsService
{
    Task<DiscountPollingResultResponse> PollDiscountsAsync();

    Task<IReadOnlyCollection<DiscountedGameResponse>> GetLatestFeaturedDiscountsAsync(int take = 5);

    Task<IReadOnlyCollection<GameVendorOfferResponse>> GetOffersByGameKeyAsync(string key);
}
