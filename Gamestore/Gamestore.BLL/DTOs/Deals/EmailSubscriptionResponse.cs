namespace Gamestore.BLL.DTOs.Deals;

public class EmailSubscriptionResponse
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime SubscribedAt { get; set; }
}
