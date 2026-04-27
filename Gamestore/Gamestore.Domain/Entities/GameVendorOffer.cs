namespace Gamestore.Domain.Entities;

public class GameVendorOffer
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public required string GameName { get; set; }

    public required string Vendor { get; set; }

    public required string PurchaseUrl { get; set; }

    public decimal Price { get; set; }

    public decimal TruePrice { get; set; }  // Base price

    public decimal CurrentPrice { get; set; }  // Actual price (may be discounted)

    public decimal? LastPolledPrice { get; set; }

    public DateTime? LastPolledAt { get; set; }

    public Game? Game { get; set; }

    public ICollection<GameDiscount> Discounts { get; set; } = [];
}
