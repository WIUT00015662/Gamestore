namespace Gamestore.BLL.DTOs.Deals;

public class DiscountConfigurationResponse
{
    public Guid Id { get; set; }

    public decimal DiscountProbability { get; set; }

    public decimal DiscountPercentageMin { get; set; }

    public decimal DiscountPercentageMax { get; set; }

    public int TimeWindowMinutes { get; set; }

    public decimal DiscountRevertProbability { get; set; }

    public bool IsActive { get; set; }
}
