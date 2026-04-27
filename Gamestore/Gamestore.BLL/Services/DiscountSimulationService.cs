namespace Gamestore.BLL.Services;

public class DiscountSimulationService : IDiscountSimulationService
{
    /// <summary>
    /// Generates a discount percent between 5-60% with 45% probability.
    /// </summary>
    public decimal GenerateDiscountPercent()
    {
        var shouldDiscount = Random.Shared.NextDouble() < 0.45;
        return !shouldDiscount ? 0m : Random.Shared.Next(5, 61);
    }

    /// <summary>
    /// Determines if discount should be applied based on probability.
    /// </summary>
    public bool ShouldApplyDiscount(decimal probability)
    {
        return Random.Shared.NextDouble() < (double)probability;
    }

    /// <summary>
    /// Determines if active discount should be reverted based on probability.
    /// </summary>
    public bool ShouldRevertDiscount(decimal probability)
    {
        return Random.Shared.NextDouble() < (double)probability;
    }

    /// <summary>
    /// Generates a discount percent within specified range.
    /// </summary>
    public decimal GenerateDiscountPercent(decimal minPercent, decimal maxPercent)
    {
        var min = (int)minPercent;
        var max = (int)maxPercent + 1;
        return Random.Shared.Next(min, max);
    }
}
