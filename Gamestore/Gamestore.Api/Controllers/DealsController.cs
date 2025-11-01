using Gamestore.Api.Auth;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("deals")]
public class DealsController(IGameDealsService gameDealsService) : ControllerBase
{
    private readonly IGameDealsService _gameDealsService = gameDealsService;

    [HttpGet("games/{key}/offers")]
    public async Task<IActionResult> GetGameOffers(string key)
    {
        var result = await _gameDealsService.GetOffersByGameKeyAsync(key);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedDeals([FromQuery] int take = 5)
    {
        var result = await _gameDealsService.GetLatestFeaturedDiscountsAsync(take);
        return Ok(result);
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost("poll")]
    public async Task<IActionResult> PollDiscounts()
    {
        var result = await _gameDealsService.PollDiscountsAsync();
        return Ok(result);
    }
}
