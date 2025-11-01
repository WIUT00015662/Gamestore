using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

/// <summary>
/// Genres controller.
/// </summary>
[ApiController]
[Route("[controller]")]
public class GenresController(IGenreService genreService, IGameService gameService) : ControllerBase
{
    private readonly IGenreService _genreService = genreService;
    private readonly IGameService _gameService = gameService;

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] CreateGenreRequest request)
    {
        var genre = await _genreService.CreateGenreAsync(request);
        return CreatedAtAction(nameof(GetGenreById), new { id = genre.Id }, genre);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenreById(Guid id)
    {
        var genre = await _genreService.GetGenreByIdAsync(id);
        return Ok(genre);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGenres()
    {
        var genres = await _genreService.GetAllGenresAsync();
        return Ok(genres);
    }

    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetGenresByParentId(Guid id)
    {
        var genres = await _genreService.GetGenresByParentIdAsync(id);
        return Ok(genres);
    }

    [HttpGet("{id}/games")]
    public async Task<IActionResult> GetGamesByGenreId(Guid id)
    {
        var games = await _gameService.GetGamesByGenreIdAsync(id);
        return Ok(games);
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPut]
    public async Task<IActionResult> UpdateGenre([FromBody] UpdateGenreRequest request)
    {
        await _genreService.UpdateGenreAsync(request);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre(Guid id)
    {
        await _genreService.DeleteGenreAsync(id);
        return NoContent();
    }
}
