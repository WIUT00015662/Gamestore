namespace Gamestore.BLL.DTOs.Deals;

public class DiscountPollingResultResponse
{
    public required Guid PollingRunId { get; set; }

    public required DateTime PolledAt { get; set; }

    public int TotalDiscountedGames { get; set; }

    public int FeaturedGamesCount { get; set; }
}
