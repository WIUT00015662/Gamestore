using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Models;

public class GetGamesQueryRequest
{
    [FromQuery(Name = "genres")]
    public Guid[]? GenreIds { get; set; }

    [FromQuery(Name = "platforms")]
    public Guid[]? PlatformIds { get; set; }

    [FromQuery(Name = "publishers")]
    public Guid[]? PublisherIds { get; set; }

    [FromQuery(Name = "minPrice")]
    public double? MinPrice { get; set; }

    [FromQuery(Name = "maxPrice")]
    public double? MaxPrice { get; set; }

    [FromQuery(Name = "datePublishing")]
    public string? PublishDateFilter { get; set; }

    [FromQuery(Name = "name")]
    public string? GameName { get; set; }

    [FromQuery(Name = "sort")]
    public string? SortBy { get; set; }

    [FromQuery(Name = "pageCount")]
    public string? PageSize { get; set; }

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;
}
