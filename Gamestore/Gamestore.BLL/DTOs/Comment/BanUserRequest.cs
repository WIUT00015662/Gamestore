namespace Gamestore.BLL.DTOs.Comment;

public class BanUserRequest
{
    public Guid UserId { get; set; }

    public BanDurationType? Duration { get; set; }
}
