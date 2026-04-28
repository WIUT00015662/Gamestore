using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Game;

public class GameVendorOfferRequest
{
    [StringLength(200, MinimumLength = 1)]
    public required string Vendor { get; set; }

    [StringLength(1000, MinimumLength = 1)]
    public required string PurchaseUrl { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ReferencePrice { get; set; }
}
