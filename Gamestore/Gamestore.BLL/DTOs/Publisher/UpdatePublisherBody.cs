using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Publisher;

public class UpdatePublisherBody
{
    public Guid Id { get; set; }

    [StringLength(200, MinimumLength = 1)]
    public required string CompanyName { get; set; }

    [StringLength(2048, MinimumLength = 1)]
    public required string HomePage { get; set; }

    [StringLength(2000, MinimumLength = 1)]
    public required string Description { get; set; }
}
