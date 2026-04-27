namespace Gamestore.Domain.Entities;

public class GameDiscount
{
    public Guid Id { get; set; }

    public Guid GameVendorOfferId { get; set; }

    public decimal DiscountPercent { get; set; }

    public bool IsCurrentlyActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevertedAt { get; set; }

    public GameVendorOffer? GameVendorOffer { get; set; }
}
