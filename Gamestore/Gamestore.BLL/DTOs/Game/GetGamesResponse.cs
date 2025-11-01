namespace Gamestore.BLL.DTOs.Game;

public class GetGamesResponse
{
    public required List<GameResponse> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}
