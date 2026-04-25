using System.Security.Claims;
using Gamestore.Api.Auth;
using Gamestore.Api.Controllers;
using Gamestore.Api.Models;
using Gamestore.Api.Services;
using Gamestore.BLL.DTOs.Comment;
using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.DTOs.Publisher;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class GamesControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IGenreService> _genreServiceMock;
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly Mock<IPublisherService> _publisherServiceMock;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GamesController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GamesControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _genreServiceMock = new Mock<IGenreService>();
        _platformServiceMock = new Mock<IPlatformService>();
        _publisherServiceMock = new Mock<IPublisherService>();
        _orderServiceMock = new Mock<IOrderService>();
        _commentServiceMock = new Mock<ICommentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(s => s.GetUserId()).Returns(_testUserId);
        _currentUserServiceMock.Setup(s => s.GetUserName()).Returns("AuthorizedUser");
        _currentUserServiceMock.Setup(s => s.HasPermission(It.IsAny<string>())).Returns(false);

        _controller = new GamesController(
            _gameServiceMock.Object,
            _genreServiceMock.Object,
            _platformServiceMock.Object,
            _publisherServiceMock.Object,
            _orderServiceMock.Object,
            _commentServiceMock.Object,
            _currentUserServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()),
                },
            },
        };
    }

    [Fact]
    public async Task CreateGameReturnsCreatedAtActionWhenValid()
    {
        var request = new CreateGameRequest
        {
            Game = new CreateGameBody
            {
                Name = "Test",
                Key = "test",
                Description = "desc",
                Price = 1,
                UnitInStock = 1,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
        };
        var response = new GameResponse { Id = Guid.NewGuid(), Name = "Test", Key = "test" };
        _gameServiceMock.Setup(s => s.CreateGameAsync(request)).ReturnsAsync(response);

        var result = await _controller.CreateGame(request);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetGameByKey), actionResult.ActionName);
        Assert.Equal("test", actionResult.RouteValues?["key"]);
        Assert.Equal(response, actionResult.Value);
    }

    [Fact]
    public async Task GetGameByKeyReturnsOkWithGame()
    {
        var response = new GameResponse { Id = Guid.NewGuid(), Name = "Test", Key = "test" };
        _gameServiceMock.Setup(s => s.GetGameByKeyAsync("test")).ReturnsAsync(response);

        var result = await _controller.GetGameByKey("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetAllGamesReturnsOkWithGames()
    {
        var responses = new List<GameResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Test 1", Key = "test-1" },
            new() { Id = Guid.NewGuid(), Name = "Test 2", Key = "test-2" },
        };
        var response = new GetGamesResponse { Games = responses, TotalPages = 1, CurrentPage = 1 };
        _gameServiceMock.Setup(s => s.GetGamesWithFiltersAsync(It.IsAny<GameFilterRequest>())).ReturnsAsync(response);

        var result = await _controller.GetAllGames(new GetGamesQueryRequest { PageSize = "10", PageNumber = 1 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<GetGamesResponse>(okResult.Value);
        Assert.Equal(2, returnedResponse.Games.Count);
    }

    [Fact]
    public async Task GetGameByIdReturnsOkWithGame()
    {
        var id = Guid.NewGuid();
        var response = new GameResponse { Id = id, Name = "Test", Key = "test" };
        _gameServiceMock.Setup(s => s.GetGameByIdAsync(id)).ReturnsAsync(response);

        var result = await _controller.GetGameById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetGenresByGameKeyReturnsOkWithGenres()
    {
        var responses = new List<GenreResponse> { new() { Id = Guid.NewGuid(), Name = "RPG" } };
        _genreServiceMock.Setup(s => s.GetGenresByGameKeyAsync("test")).ReturnsAsync(responses);

        var result = await _controller.GetGenresByGameKey("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task GetPlatformsByGameKeyReturnsOkWithPlatforms()
    {
        var responses = new List<PlatformResponse> { new() { Id = Guid.NewGuid(), Type = "PC" } };
        _platformServiceMock.Setup(s => s.GetPlatformsByGameKeyAsync("test")).ReturnsAsync(responses);

        var result = await _controller.GetPlatformsByGameKey("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, okResult.Value);
    }

    [Fact]
    public async Task GetPublisherByGameKeyReturnsOkWithPublisher()
    {
        var response = new PublisherResponse { Id = Guid.NewGuid(), CompanyName = "Pub" };
        _publisherServiceMock.Setup(s => s.GetPublisherByGameKeyAsync("test")).ReturnsAsync(response);

        var result = await _controller.GetPublisherByGameKey("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task UpdateGameReturnsNoContent()
    {
        var request = new UpdateGameRequest
        {
            Game = new UpdateGameBody
            {
                Id = Guid.NewGuid(),
                Name = "Update",
                Key = "up",
                Description = "desc",
                Price = 1,
                UnitInStock = 1,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
        };
        _gameServiceMock.Setup(s => s.UpdateGameAsync(request, It.IsAny<bool>())).Returns(Task.CompletedTask);

        var result = await _controller.UpdateGame(request);

        Assert.IsType<NoContentResult>(result);
        _gameServiceMock.Verify(s => s.UpdateGameAsync(request, false), Times.Once);
    }

    [Fact]
    public async Task DeleteGameReturnsNoContent()
    {
        _gameServiceMock.Setup(s => s.DeleteGameAsync("test")).Returns(Task.CompletedTask);

        var result = await _controller.DeleteGame("test");

        Assert.IsType<NoContentResult>(result);
        _gameServiceMock.Verify(s => s.DeleteGameAsync("test"), Times.Once);
    }

    [Fact]
    public async Task BuyGameReturnsNoContent()
    {
        _orderServiceMock.Setup(s => s.AddGameToCartAsync("test", _testUserId)).Returns(Task.CompletedTask);

        var result = await _controller.BuyGame("test");

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(s => s.AddGameToCartAsync("test", _testUserId), Times.Once);
    }

    [Fact]
    public async Task AddCommentReturnsNoContent()
    {
        var request = new AddCommentRequest
        {
            Comment = new CommentBodyDto { Name = "John", Body = "Hello" },
        };
        _commentServiceMock.Setup(x => x.AddCommentAsync("test", request, _testUserId, "AuthorizedUser")).Returns(Task.CompletedTask);

        var result = await _controller.AddComment("test", request);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.AddCommentAsync("test", request, _testUserId, "AuthorizedUser"), Times.Once);
    }

    [Fact]
    public async Task GetCommentsReturnsOkWithComments()
    {
        var comments = new List<CommentResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "John", Body = "Root" },
        };
        _commentServiceMock.Setup(x => x.GetCommentsByGameKeyAsync("test")).ReturnsAsync(comments);

        var result = await _controller.GetComments("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(comments, okResult.Value);
    }

    [Fact]
    public async Task DeleteCommentReturnsNoContent()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.DeleteCommentAsync("test", commentId, _testUserId, "AuthorizedUser", false)).Returns(Task.CompletedTask);

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "AuthorizedUser")], "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
            },
        };

        var result = await _controller.DeleteComment("test", commentId);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.DeleteCommentAsync("test", commentId, _testUserId, "AuthorizedUser", false), Times.Once);
    }

    [Fact]
    public void GetPaginationOptionsReturnsOkWithOptions()
    {
        var options = new List<string> { "10", "20", "50", "100", "all" };
        _gameServiceMock.Setup(s => s.GetPaginationOptions()).Returns(options);

        var result = _controller.GetPaginationOptions();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(options, okResult.Value);
    }

    [Fact]
    public void GetSortingOptionsReturnsOkWithOptions()
    {
        var options = new List<string> { "Most popular", "Most commented", "Price ASC", "Price DESC", "New" };
        _gameServiceMock.Setup(s => s.GetSortingOptions()).Returns(options);

        var result = _controller.GetSortingOptions();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(options, okResult.Value);
    }

    [Fact]
    public void GetPublishDateFilterOptionsReturnsOkWithOptions()
    {
        var options = new List<string> { "last week", "last month", "last year", "2 years", "3 years" };
        _gameServiceMock.Setup(s => s.GetPublishDateFilterOptions()).Returns(options);

        var result = _controller.GetPublishDateFilterOptions();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(options, okResult.Value);
    }

    [Fact]
    public async Task GetAllGamesWithFiltersReturnsOkWithFilteredGames()
    {
        var gameResponse = new GameResponse { Id = Guid.NewGuid(), Name = "Test Game", Key = "test-game" };
        var response = new GetGamesResponse { Games = [gameResponse], TotalPages = 1, CurrentPage = 1 };

        _gameServiceMock.Setup(s => s.GetGamesWithFiltersAsync(It.IsAny<GameFilterRequest>())).ReturnsAsync(response);

        var result = await _controller.GetAllGames(new GetGamesQueryRequest { PageSize = "10", PageNumber = 1 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<GetGamesResponse>(okResult.Value);
        Assert.Single(returnedResponse.Games);
        Assert.Equal(1, returnedResponse.CurrentPage);
    }

    [Fact]
    public async Task GetGameByKeyIncrementsViewCount()
    {
        var response = new GameResponse { Id = Guid.NewGuid(), Name = "Test", Key = "test" };
        _gameServiceMock.Setup(s => s.GetGameByKeyAsync("test")).ReturnsAsync(response);
        _gameServiceMock.Setup(s => s.IncrementGameViewCountAsync(response.Id)).Returns(Task.CompletedTask);

        var result = await _controller.GetGameByKey("test");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
        _gameServiceMock.Verify(s => s.IncrementGameViewCountAsync(response.Id), Times.Once);
    }

    [Fact]
    public async Task GetAllGamesWithoutFiltersReturnsOkWithGamesWhenUserHasNoClaim()
    {
        var games = new List<GameResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Test 1", Key = "test-1" },
        };

        _gameServiceMock.Setup(s => s.GetAllGamesAsync(false)).ReturnsAsync(games);
        _currentUserServiceMock.Setup(s => s.HasPermission(Permissions.ViewDeletedGames)).Returns(false);

        var result = await _controller.GetAllGamesWithoutFilters();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(games, okResult.Value);
        _gameServiceMock.Verify(s => s.GetAllGamesAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetAllGamesWithoutFiltersReturnsDeletedGamesWhenUserHasViewDeletedClaim()
    {
        var games = new List<GameResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Deleted Test", Key = "deleted-test" },
        };

        _gameServiceMock.Setup(s => s.GetAllGamesAsync(true)).ReturnsAsync(games);
        _currentUserServiceMock.Setup(s => s.HasPermission(Permissions.ViewDeletedGames)).Returns(true);

        var result = await _controller.GetAllGamesWithoutFilters();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(games, okResult.Value);
        _gameServiceMock.Verify(s => s.GetAllGamesAsync(true), Times.Once);
    }

    [Fact]
    public async Task AddCommentUsesAuthenticatedUserNameWhenPresent()
    {
        var request = new AddCommentRequest
        {
            Comment = new CommentBodyDto { Name = "Original", Body = "Hello" },
        };

        _commentServiceMock.Setup(x => x.AddCommentAsync("test", request, _testUserId, "AuthorizedUser")).Returns(Task.CompletedTask);

        var result = await _controller.AddComment("test", request);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.AddCommentAsync("test", request, _testUserId, "AuthorizedUser"), Times.Once);
    }

    [Fact]
    public async Task UpdateGameUsesIncludeDeletedWhenUserHasEditDeletedClaim()
    {
        var request = new UpdateGameRequest
        {
            Game = new UpdateGameBody
            {
                Id = Guid.NewGuid(),
                Name = "Update",
                Key = "up",
                Description = "desc",
                Price = 1,
                UnitInStock = 1,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
        };

        var claims = new List<Claim> { new("permission", Permissions.EditDeletedGame) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
            },
        };

        _gameServiceMock.Setup(s => s.UpdateGameAsync(request, true)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateGame(request);

        Assert.IsType<NoContentResult>(result);
        _gameServiceMock.Verify(s => s.UpdateGameAsync(request, true), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentUsesCurrentUserServiceIdentityAndPermissions()
    {
        var commentId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.GetUserName()).Returns("AuthorizedUser");
        _currentUserServiceMock.Setup(s => s.HasPermission(Permissions.ManageComments)).Returns(true);
        _commentServiceMock.Setup(x => x.DeleteCommentAsync("test", commentId, _testUserId, "AuthorizedUser", true)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteComment("test", commentId);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.DeleteCommentAsync("test", commentId, _testUserId, "AuthorizedUser", true), Times.Once);
    }
}
