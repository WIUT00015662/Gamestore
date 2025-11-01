using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Comment;
using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

/// <summary>
/// Games controller.
/// </summary>
[ApiController]
[Route("[controller]")]
public class GamesController(
    IGameService gameService,
    IGenreService genreService,
    IPlatformService platformService,
    IPublisherService publisherService,
    IOrderService orderService,
    ICommentService commentService) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IGenreService _genreService = genreService;
    private readonly IPlatformService _platformService = platformService;
    private readonly IPublisherService _publisherService = publisherService;
    private readonly IOrderService _orderService = orderService;
    private readonly ICommentService _commentService = commentService;

    [Authorize(Policy = Permissions.AddGame)]
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = await _gameService.CreateGameAsync(request);
        return CreatedAtAction(nameof(GetGameByKey), new { key = game.Key }, game);
    }

    [HttpGet("find/{id}")]
    public async Task<IActionResult> GetGameById(Guid id)
    {
        var game = await _gameService.GetGameByIdAsync(id);
        return Ok(game);
    }

    [HttpGet("pagination-options")]
    public IActionResult GetPaginationOptions()
    {
        var options = _gameService.GetPaginationOptions();
        return Ok(options);
    }

    [HttpGet("sorting-options")]
    public IActionResult GetSortingOptions()
    {
        var options = _gameService.GetSortingOptions();
        return Ok(options);
    }

    [HttpGet("publish-date-options")]
    [HttpGet("publish-date-filter-options")]
    public IActionResult GetPublishDateFilterOptions()
    {
        var options = _gameService.GetPublishDateFilterOptions();
        return Ok(options);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames(
        [FromQuery(Name = "genres")] Guid[]? genreIds,
        [FromQuery(Name = "platforms")] Guid[]? platformIds,
        [FromQuery(Name = "publishers")] Guid[]? publisherIds,
        [FromQuery(Name = "minPrice")] double? minPrice,
        [FromQuery(Name = "maxPrice")] double? maxPrice,
        [FromQuery(Name = "datePublishing")] string? publishDateFilter,
        [FromQuery(Name = "name")] string? gameName,
        [FromQuery(Name = "sort")] string? sortBy,
        [FromQuery(Name = "pageCount")] string? pageSize,
        [FromQuery(Name = "page")] int pageNumber = 1)
    {
        var request = new GameFilterRequest
        {
            GenreIds = genreIds?.ToList(),
            PlatformIds = platformIds?.ToList(),
            PublisherIds = publisherIds?.ToList(),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PublishDateFilter = publishDateFilter,
            GameName = gameName,
            SortBy = sortBy,
            PageSize = pageSize ?? "10",
            PageNumber = pageNumber,
        };

        var result = await _gameService.GetGamesWithFiltersAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGamesWithoutFilters()
    {
        var includeDeleted = User.HasClaim("permission", Permissions.ViewDeletedGames);
        var games = await _gameService.GetAllGamesAsync(includeDeleted);
        return Ok(games);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetGameByKey(string key)
    {
        var game = await _gameService.GetGameByKeyAsync(key);
        await _gameService.IncrementGameViewCountAsync(game.Id);
        return Ok(game);
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpPost("{key}/buy")]
    public async Task<IActionResult> BuyGame(string key)
    {
        await _orderService.AddGameToCartAsync(key);
        return NoContent();
    }

    [Authorize(Policy = Permissions.CommentGame)]
    [HttpPost("{key}/comments")]
    public async Task<IActionResult> AddComment(string key, [FromBody] AddCommentRequest request)
    {
        request.Comment.Name = User.Identity?.Name ?? request.Comment.Name;
        await _commentService.AddCommentAsync(key, request);
        return NoContent();
    }

    [HttpGet("{key}/comments")]
    public async Task<IActionResult> GetComments(string key)
    {
        var comments = await _commentService.GetCommentsByGameKeyAsync(key);
        return Ok(comments);
    }

    [Authorize(Policy = Permissions.ManageComments)]
    [HttpDelete("{key}/comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(string key, Guid id)
    {
        await _commentService.DeleteCommentAsync(key, id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.UpdateGame)]
    [HttpPut]
    public async Task<IActionResult> UpdateGame([FromBody] UpdateGameRequest request)
    {
        var includeDeleted = User.HasClaim("permission", Permissions.EditDeletedGame);
        await _gameService.UpdateGameAsync(request, includeDeleted);
        return NoContent();
    }

    [Authorize(Policy = Permissions.DeleteGame)]
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteGame(string key)
    {
        await _gameService.DeleteGameAsync(key);
        return NoContent();
    }

    [HttpGet("{key}/file")]
    public async Task<IActionResult> DownloadGameFile(string key)
    {
        var file = await _gameService.DownloadGameFileAsync(key);
        return File(file.Content, "text/plain", file.FileName);
    }

    [HttpGet("{key}/genres")]
    public async Task<IActionResult> GetGenresByGameKey(string key)
    {
        var genres = await _genreService.GetGenresByGameKeyAsync(key);
        return Ok(genres);
    }

    [HttpGet("{key}/platforms")]
    public async Task<IActionResult> GetPlatformsByGameKey(string key)
    {
        var platforms = await _platformService.GetPlatformsByGameKeyAsync(key);
        return Ok(platforms);
    }

    [HttpGet("{key}/publisher")]
    public async Task<IActionResult> GetPublisherByGameKey(string key)
    {
        var publisher = await _publisherService.GetPublisherByGameKeyAsync(key);
        return Ok(publisher);
    }
}
