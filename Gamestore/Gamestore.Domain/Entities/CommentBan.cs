namespace Gamestore.Domain.Entities;

public class CommentBan
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public DateTime? BannedUntil { get; set; }

    public bool IsPermanent { get; set; }

    public User? User { get; set; }
}
