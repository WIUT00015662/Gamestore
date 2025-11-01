using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Game;

public class CreateGameBody
{
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Key must contain only lowercase letters, numbers, and hyphens.")]
    public required string Key { get; set; }

    [StringLength(2000, MinimumLength = 1)]
    public required string Description { get; set; }

    [Range(0, double.MaxValue)]
    public required double Price { get; set; }

    [Range(0, int.MaxValue)]
    public required int UnitInStock { get; set; }

    [Range(0, int.MaxValue)]
    public required int Discount { get; set; }
}
