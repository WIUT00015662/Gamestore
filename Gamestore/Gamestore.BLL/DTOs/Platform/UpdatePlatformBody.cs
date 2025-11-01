using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Platform;

public class UpdatePlatformBody
{
    public required Guid Id { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public required string Type { get; set; }
}
