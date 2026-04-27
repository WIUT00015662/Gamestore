namespace Gamestore.Domain.Entities;

public class EmailSubscription
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnsubscribedAt { get; set; }
}
