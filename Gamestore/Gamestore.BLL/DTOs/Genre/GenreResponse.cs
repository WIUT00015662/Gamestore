namespace Gamestore.BLL.DTOs.Genre;

public class GenreResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
