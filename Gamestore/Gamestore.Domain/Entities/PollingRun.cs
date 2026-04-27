namespace Gamestore.Domain.Entities;

public class PollingRun
{
    public Guid Id { get; set; }

    public DateTime RunAt { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";  // Pending, Running, Completed, Failed

    public int ProcessedOffersCount { get; set; }

    public int NewDiscountsCreated { get; set; }

    public int DiscountsReverted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
