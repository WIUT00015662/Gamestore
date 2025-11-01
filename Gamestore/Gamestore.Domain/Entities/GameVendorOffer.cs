namespace Gamestore.Domain.Entities;

public class GameVendorOffer
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public required string GameName { get; set; }

    public required string Vendor { get; set; }

    public required string PurchaseUrl { get; set; }

    public decimal Price { get; set; }

    public decimal? LastPolledPrice { get; set; }

    public DateTime? LastPolledAt { get; set; }

    public Game? Game { get; set; }
}
