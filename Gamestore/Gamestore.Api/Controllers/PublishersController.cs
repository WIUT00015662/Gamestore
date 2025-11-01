using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Publisher;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PublishersController(IPublisherService publisherService, IGameService gameService) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;
    private readonly IGameService _gameService = gameService;

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPost]
    public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest request)
    {
        var publisher = await _publisherService.CreatePublisherAsync(request);
        return CreatedAtAction(nameof(GetPublisherByCompanyName), new { companyName = publisher.CompanyName }, publisher);
    }

    [HttpGet("{companyName}")]
    public async Task<IActionResult> GetPublisherByCompanyName(string companyName)
    {
        var publisher = await _publisherService.GetPublisherByCompanyNameAsync(companyName);
        return Ok(publisher);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPublishers()
    {
        var publishers = await _publisherService.GetAllPublishersAsync();
        return Ok(publishers);
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpPut]
    public async Task<IActionResult> UpdatePublisher([FromBody] UpdatePublisherRequest request)
    {
        await _publisherService.UpdatePublisherAsync(request);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageEntities)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePublisher(Guid id)
    {
        await _publisherService.DeletePublisherAsync(id);
        return NoContent();
    }

    [HttpGet("{companyName}/games")]
    public async Task<IActionResult> GetGamesByPublisherName(string companyName)
    {
        var games = await _gameService.GetGamesByPublisherNameAsync(companyName);
        return Ok(games);
    }
}
