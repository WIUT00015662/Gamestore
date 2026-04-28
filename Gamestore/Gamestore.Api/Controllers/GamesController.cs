using Gamestore.Api.Auth;
using Gamestore.Api.Models;
using Gamestore.Api.Services;
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
[Route("games")]
public class GamesController(
    IGameService gameService,
    IGenreService genreService,
    IPlatformService platformService,
    IPublisherService publisherService,
    IOrderService orderService,
    ICommentService commentService,
    ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IGenreService _genreService = genreService;
    private readonly IPlatformService _platformService = platformService;
    private readonly IPublisherService _publisherService = publisherService;
    private readonly IOrderService _orderService = orderService;
    private readonly ICommentService _commentService = commentService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

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
    public async Task<IActionResult> GetAllGames([FromQuery] GetGamesQueryRequest query)
    {
        var request = new GameFilterRequest
        {
            GenreIds = query.GenreIds?.ToList(),
            PlatformIds = query.PlatformIds?.ToList(),
            PublisherIds = query.PublisherIds?.ToList(),
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            PublishDateFilter = query.PublishDateFilter,
            GameName = query.GameName,
            SortBy = query.SortBy,
            PageSize = query.PageSize ?? "10",
            PageNumber = query.PageNumber,
        };

        var result = await _gameService.GetGamesWithFiltersAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGamesWithoutFilters()
    {
        var games = await _gameService.GetAllGamesAsync();
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
        var userId = _currentUserService.GetUserId();
        await _orderService.AddGameToCartAsync(key, userId);
        return NoContent();
    }

    [Authorize(Policy = Permissions.CommentGame)]
    [HttpPost("{key}/comments")]
    public async Task<IActionResult> AddComment(string key, [FromBody] AddCommentRequest request)
    {
        var actorUserId = _currentUserService.GetUserId();
        var actorName = _currentUserService.GetUserName();
        await _commentService.AddCommentAsync(key, request, actorUserId, actorName);
        return NoContent();
    }

    [HttpGet("{key}/comments")]
    public async Task<IActionResult> GetComments(string key)
    {
        var comments = await _commentService.GetCommentsByGameKeyAsync(key);
        return Ok(comments);
    }

    [Authorize(Policy = Permissions.UpdateGame)]
    [HttpPut]
    public async Task<IActionResult> UpdateGame([FromBody] UpdateGameRequest request)
    {
        await _gameService.UpdateGameAsync(request);
        return NoContent();
    }

    [Authorize(Policy = Permissions.DeleteGame)]
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteGame(string key)
    {
        await _gameService.DeleteGameAsync(key);
        return NoContent();
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

    [Authorize(Policy = Permissions.CommentGame)]
    [HttpPut("{key}/comments/{id:guid}")]
    public async Task<IActionResult> UpdateComment(string key, Guid id, [FromBody] UpdateCommentRequest request)
    {
        var actorUserId = _currentUserService.GetUserId();
        var actorName = _currentUserService.GetUserName();
        await _commentService.UpdateCommentAsync(key, id, request, actorUserId, actorName);
        return NoContent();
    }

    [Authorize(Policy = Permissions.CommentGame)]
    [HttpDelete("{key}/comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(string key, Guid id)
    {
        var actorUserId = _currentUserService.GetUserId();
        var actorName = _currentUserService.GetUserName();
        var canManageComments = _currentUserService.HasPermission(Permissions.ManageComments);
        await _commentService.DeleteCommentAsync(key, id, actorUserId, actorName, canManageComments);
        return NoContent();
    }
}
