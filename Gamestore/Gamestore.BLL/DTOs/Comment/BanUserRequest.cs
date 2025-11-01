namespace Gamestore.BLL.DTOs.Comment;

public class BanUserRequest
{
    public required string User { get; set; }

    public required string Duration { get; set; }
}
