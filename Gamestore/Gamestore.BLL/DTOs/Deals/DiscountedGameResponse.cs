namespace Gamestore.BLL.DTOs.Deals;

public class DiscountedGameResponse
{
    public required Guid GameId { get; set; }

    public required string GameName { get; set; }

    public required string Vendor { get; set; }

    public required string PurchaseUrl { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal DiscountedPrice { get; set; }

    public decimal DiscountPercent { get; set; }
}
