namespace Gamestore.BLL.DTOs.Game;

public class CreateGameRequest
{
    public required CreateGameBody Game { get; set; }

    public List<GameVendorOfferRequest> VendorOffers { get; set; } = [];

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];

    public Guid Publisher { get; set; }
}
