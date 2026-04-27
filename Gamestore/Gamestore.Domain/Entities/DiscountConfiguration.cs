namespace Gamestore.Domain.Entities;

public class DiscountConfiguration
{
    public Guid Id { get; set; }

    public decimal DiscountProbability { get; set; } = 0.3m;  // 30% chance

    public decimal DiscountPercentageMin { get; set; } = 5m;

    public decimal DiscountPercentageMax { get; set; } = 50m;

    public int TimeWindowMinutes { get; set; } = 30;

    public decimal DiscountRevertProbability { get; set; } = 0.4m;  // 40% chance to revert

    public bool IsActive { get; set; } = true;
}
