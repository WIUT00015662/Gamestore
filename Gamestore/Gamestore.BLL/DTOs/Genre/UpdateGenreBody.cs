using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Gamestore.BLL.Serialization;

namespace Gamestore.BLL.DTOs.Genre;

public class UpdateGenreBody
{
    public required Guid Id { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [JsonConverter(typeof(NullableGuidEmptyStringJsonConverter))]
    public Guid? ParentGenreId { get; set; }
}
