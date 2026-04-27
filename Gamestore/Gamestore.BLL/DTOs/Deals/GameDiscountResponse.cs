namespace Gamestore.BLL.DTOs.Deals;

public class GameDiscountResponse
{
    public Guid Id { get; set; }

    public Guid GameVendorOfferId { get; set; }

    public decimal DiscountPercent { get; set; }

    public bool IsCurrentlyActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevertedAt { get; set; }
}
