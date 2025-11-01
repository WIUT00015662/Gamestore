namespace Gamestore.BLL.DTOs.Deals;

public class GameVendorOfferResponse
{
    public required Guid Id { get; set; }

    public required string Vendor { get; set; }

    public required string PurchaseUrl { get; set; }

    public decimal Price { get; set; }

    public decimal? LastPolledPrice { get; set; }

    public DateTime? LastPolledAt { get; set; }
}
