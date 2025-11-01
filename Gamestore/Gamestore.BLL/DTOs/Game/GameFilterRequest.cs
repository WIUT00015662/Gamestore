namespace Gamestore.BLL.DTOs.Game;

public class GameFilterRequest
{
    public List<Guid>? GenreIds { get; set; }

    public List<Guid>? PlatformIds { get; set; }

    public List<Guid>? PublisherIds { get; set; }

    public double? MinPrice { get; set; }

    public double? MaxPrice { get; set; }

    public string? PublishDateFilter { get; set; }

    public string? GameName { get; set; }

    public string? SortBy { get; set; }

    public string? PageSize { get; set; } = "10";

    public int PageNumber { get; set; } = 1;
}
