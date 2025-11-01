using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Platform;

public class CreatePlatformBody
{
    [StringLength(100, MinimumLength = 1)]
    public required string Type { get; set; }
}
