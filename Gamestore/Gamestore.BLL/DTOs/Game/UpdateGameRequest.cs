namespace Gamestore.BLL.DTOs.Game;

public class UpdateGameRequest
{
    public required UpdateGameBody Game { get; set; }

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];

    public Guid Publisher { get; set; }
}
