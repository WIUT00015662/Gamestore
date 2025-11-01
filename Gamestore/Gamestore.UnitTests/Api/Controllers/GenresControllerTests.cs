using Gamestore.Api.Controllers;
using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class GenresControllerTests
{
    private readonly Mock<IGenreService> _genreServiceMock;
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly GenresController _controller;

    public GenresControllerTests()
    {
        _genreServiceMock = new Mock<IGenreService>();
        _gameServiceMock = new Mock<IGameService>();

        _controller = new GenresController(
            _genreServiceMock.Object,
            _gameServiceMock.Object);
    }

    [Fact]
    public async Task CreateGenreReturnsCreatedAtActionWhenValid()
    {
        var request = new CreateGenreRequest { Genre = new CreateGenreBody { Name = "RPG" } };
        var id = Guid.NewGuid();
        var response = new GenreResponse { Id = id, Name = "RPG" };
        _genreServiceMock.Setup(s => s.CreateGenreAsync(request)).ReturnsAsync(response);

        var result = await _controller.CreateGenre(request);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetGenreById), actionResult.ActionName);
        Assert.Equal(id, actionResult.RouteValues?["id"]);
        Assert.Equal(response, actionResult.Value);
    }

    [Fact]
    public async Task GetGenreByIdReturnsOkWithGenre()
    {
        var id = Guid.NewGuid();
        var response = new GenreResponse { Id = id, Name = "RPG" };
        _genreServiceMock.Setup(s => s.GetGenreByIdAsync(id)).ReturnsAsync(response);

        var result = await _controller.GetGenreById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetAllGenresReturnsOkWithGenres()
    {
        var responses = new List<GenreResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "RPG" },
        };
        _genreServiceMock.Setup(s => s.GetAllGenresAsync()).ReturnsAsync(responses);

        var result = await _controller.GetAllGenres();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task GetGenresByParentIdReturnsOkWithGenres()
    {
        var parentId = Guid.NewGuid();
        var responses = new List<GenreResponse> { new() { Id = Guid.NewGuid(), Name = "Action RPG", ParentGenreId = parentId } };
        _genreServiceMock.Setup(s => s.GetGenresByParentIdAsync(parentId)).ReturnsAsync(responses);

        var result = await _controller.GetGenresByParentId(parentId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task GetGamesByGenreIdReturnsOkWithGames()
    {
        var genreId = Guid.NewGuid();
        var responses = new List<GameResponse> { new() { Id = Guid.NewGuid(), Name = "Witcher 3", Key = "witcher-3" } };
        _gameServiceMock.Setup(s => s.GetGamesByGenreIdAsync(genreId)).ReturnsAsync(responses);

        var result = await _controller.GetGamesByGenreId(genreId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task UpdateGenreReturnsNoContent()
    {
        var request = new UpdateGenreRequest { Genre = new UpdateGenreBody { Id = Guid.NewGuid(), Name = "Update" } };
        _genreServiceMock.Setup(s => s.UpdateGenreAsync(request)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateGenre(request);

        Assert.IsType<NoContentResult>(result);
        _genreServiceMock.Verify(s => s.UpdateGenreAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteGenreReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _genreServiceMock.Setup(s => s.DeleteGenreAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteGenre(id);

        Assert.IsType<NoContentResult>(result);
        _genreServiceMock.Verify(s => s.DeleteGenreAsync(id), Times.Once);
    }
}
