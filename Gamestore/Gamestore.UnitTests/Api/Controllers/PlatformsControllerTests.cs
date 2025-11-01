using Gamestore.Api.Controllers;
using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class PlatformsControllerTests
{
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly PlatformsController _controller;

    public PlatformsControllerTests()
    {
        _platformServiceMock = new Mock<IPlatformService>();
        _gameServiceMock = new Mock<IGameService>();

        _controller = new PlatformsController(
            _platformServiceMock.Object,
            _gameServiceMock.Object);
    }

    [Fact]
    public async Task CreatePlatformReturnsCreatedAtActionWhenValid()
    {
        var request = new CreatePlatformRequest { Platform = new CreatePlatformBody { Type = "PC" } };
        var id = Guid.NewGuid();
        var response = new PlatformResponse { Id = id, Type = "PC" };
        _platformServiceMock.Setup(s => s.CreatePlatformAsync(request)).ReturnsAsync(response);

        var result = await _controller.CreatePlatform(request);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetPlatformById), actionResult.ActionName);
        Assert.Equal(id, actionResult.RouteValues?["id"]);
        Assert.Equal(response, actionResult.Value);
    }

    [Fact]
    public async Task GetPlatformByIdReturnsOkWithPlatform()
    {
        var id = Guid.NewGuid();
        var response = new PlatformResponse { Id = id, Type = "PC" };
        _platformServiceMock.Setup(s => s.GetPlatformByIdAsync(id)).ReturnsAsync(response);

        var result = await _controller.GetPlatformById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetAllPlatformsReturnsOkWithPlatforms()
    {
        var responses = new List<PlatformResponse>
        {
            new() { Id = Guid.NewGuid(), Type = "PC" },
        };
        _platformServiceMock.Setup(s => s.GetAllPlatformsAsync()).ReturnsAsync(responses);

        var result = await _controller.GetAllPlatforms();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task GetGamesByPlatformIdReturnsOkWithGames()
    {
        var platformId = Guid.NewGuid();
        var responses = new List<GameResponse> { new() { Id = Guid.NewGuid(), Name = "Witcher 3", Key = "witcher-3" } };
        _gameServiceMock.Setup(s => s.GetGamesByPlatformIdAsync(platformId)).ReturnsAsync(responses);

        var result = await _controller.GetGamesByPlatformId(platformId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task UpdatePlatformReturnsNoContent()
    {
        var request = new UpdatePlatformRequest { Platform = new UpdatePlatformBody { Id = Guid.NewGuid(), Type = "Mobile" } };
        _platformServiceMock.Setup(s => s.UpdatePlatformAsync(request)).Returns(Task.CompletedTask);

        var result = await _controller.UpdatePlatform(request);

        Assert.IsType<NoContentResult>(result);
        _platformServiceMock.Verify(s => s.UpdatePlatformAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _platformServiceMock.Setup(s => s.DeletePlatformAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.DeletePlatform(id);

        Assert.IsType<NoContentResult>(result);
        _platformServiceMock.Verify(s => s.DeletePlatformAsync(id), Times.Once);
    }
}
