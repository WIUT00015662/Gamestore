using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Deals;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("deals")]
public class DealsController(IGameDealsService gameDealsService, IEmailService emailService) : ControllerBase
{
    private readonly IGameDealsService _gameDealsService = gameDealsService;
    private readonly IEmailService _emailService = emailService;

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

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCurrentDiscounts()
    {
        var result = await _gameDealsService.GetAllCurrentDiscountsAsync();
        return Ok(result);
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost("poll")]
    public async Task<IActionResult> PollDiscounts()
    {
        var result = await _gameDealsService.PollDiscountsAsync();
        return Ok(result);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> SubscribeEmail([FromBody] EmailSubscriptionRequest request)
    {
        var result = await _gameDealsService.SubscribeEmailAsync(request.Email);
        return Ok(result);
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> UnsubscribeEmail([FromBody] EmailSubscriptionRequest request)
    {
        await _gameDealsService.UnsubscribeEmailAsync(request.Email);
        return Ok();
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] EmailSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        await _emailService.SendEmailAsync(
            request.Email,
            "Gamestore SMTP test",
            "<p>This is a test email from Gamestore.</p>");

        return Ok();
    }
}
