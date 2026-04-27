namespace Gamestore.BLL.DTOs.Deals;

public class PollingRunResponse
{
    public Guid Id { get; set; }

    public DateTime RunAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ProcessedOffersCount { get; set; }

    public int NewDiscountsCreated { get; set; }

    public int DiscountsReverted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
