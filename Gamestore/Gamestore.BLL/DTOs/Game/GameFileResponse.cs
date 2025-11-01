namespace Gamestore.BLL.DTOs.Game;

public class GameFileResponse
{
    public required byte[] Content { get; set; }

    public required string FileName { get; set; }
}
