using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

/// <summary>
/// Platforms controller.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PlatformsController(IPlatformService platformService, IGameService gameService) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly IGameService _gameService = gameService;

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost]
    public async Task<IActionResult> CreatePlatform([FromBody] CreatePlatformRequest request)
    {
        var platform = await _platformService.CreatePlatformAsync(request);
        return CreatedAtAction(nameof(GetPlatformById), new { id = platform.Id }, platform);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlatformById(Guid id)
    {
        var platform = await _platformService.GetPlatformByIdAsync(id);
        return Ok(platform);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlatforms()
    {
        var platforms = await _platformService.GetAllPlatformsAsync();
        return Ok(platforms);
    }

    [HttpGet("{id}/games")]
    public async Task<IActionResult> GetGamesByPlatformId(Guid id)
    {
        var games = await _gameService.GetGamesByPlatformIdAsync(id);
        return Ok(games);
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPut]
    public async Task<IActionResult> UpdatePlatform([FromBody] UpdatePlatformRequest request)
    {
        await _platformService.UpdatePlatformAsync(request);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlatform(Guid id)
    {
        await _platformService.DeletePlatformAsync(id);
        return NoContent();
    }
}
