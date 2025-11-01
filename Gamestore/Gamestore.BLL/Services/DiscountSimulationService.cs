namespace Gamestore.BLL.Services;

public class DiscountSimulationService : IDiscountSimulationService
{
    public decimal GenerateDiscountPercent()
    {
        var shouldDiscount = Random.Shared.NextDouble() < 0.45;
        return !shouldDiscount ? 0m : Random.Shared.Next(5, 61);
    }
}
