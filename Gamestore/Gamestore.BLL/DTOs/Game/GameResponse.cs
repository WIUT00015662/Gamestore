namespace Gamestore.BLL.DTOs.Game;

public class GameResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Key { get; set; }

    public string? Description { get; set; }

    public int UnitInStock { get; set; }

    public decimal? BestOfferPrice { get; set; }
}
