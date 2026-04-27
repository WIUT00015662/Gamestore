namespace Gamestore.Domain.Entities;

public class GameDiscountSnapshot
{
    public Guid Id { get; set; }

    public Guid PollingRunId { get; set; }

    public DateTime PolledAt { get; set; }

    public Guid GameId { get; set; }

    public required string GameName { get; set; }

    public required string Vendor { get; set; }

    public required string PurchaseUrl { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal DiscountedPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsNewDiscount { get; set; }

    public Game? Game { get; set; }
}
