using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.Services;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class GameServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IGenreRepository> _genreRepoMock;
    private readonly Mock<IPlatformRepository> _platformRepoMock;
    private readonly Mock<IPublisherRepository> _publisherRepoMock;
    private readonly Mock<IRepository<GameVendorOffer>> _gameVendorOfferRepoMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _genreRepoMock = new Mock<IGenreRepository>();
        _platformRepoMock = new Mock<IPlatformRepository>();
        _publisherRepoMock = new Mock<IPublisherRepository>();
        _gameVendorOfferRepoMock = new Mock<IRepository<GameVendorOffer>>();
        _loggerMock = new Mock<ILogger<GameService>>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Games).Returns(_gameRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Genres).Returns(_genreRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Platforms).Returns(_platformRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Publishers).Returns(_publisherRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.GameVendorOffers).Returns(_gameVendorOfferRepoMock.Object);

        _gameService = new GameService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateGameAsyncThrowsEntityAlreadyExistsExceptionWhenKeyExists()
    {
        var req = new CreateGameRequest
        {
            Game = new CreateGameBody
            {
                Name = "Test",
                Key = "test-key",
                Description = "desc",
                UnitInStock = 1,
            },
            VendorOffers =
            [
                new GameVendorOfferRequest { Vendor = "Store", PurchaseUrl = "https://example.com", Price = 1, ReferencePrice = 1 },
            ],
            Publisher = Guid.NewGuid(),
        };
        _gameRepoMock.Setup(x => x.GetByKeyAsync("test-key")).ReturnsAsync(new Game { Name = "Test", Key = "test-key" });

        await Assert.ThrowsAsync<EntityAlreadyExistsException>(() => _gameService.CreateGameAsync(req));
    }

    [Fact]
    public async Task CreateGameAsyncCreatesGameAndReturnsResponse()
    {
        var publisherId = Guid.NewGuid();
        var req = new CreateGameRequest
        {
            Game = new CreateGameBody { Name = "Test Game", Key = "test-game", Description = "Desc", UnitInStock = 2 },
            VendorOffers =
            [
                new GameVendorOfferRequest { Vendor = "Store", PurchaseUrl = "https://example.com", Price = 1, ReferencePrice = 1 },
            ],
            Genres = [Guid.NewGuid()],
            Platforms = [Guid.NewGuid()],
            Publisher = publisherId,
        };

        _gameRepoMock.Setup(x => x.GetByKeyAsync("test-game")).ReturnsAsync((Game?)null);
        _genreRepoMock.Setup(x => x.GetByIdAsync(req.Genres[0])).ReturnsAsync(new Genre { Id = req.Genres[0], Name = "RPG" });
        _platformRepoMock.Setup(x => x.GetByIdAsync(req.Platforms[0])).ReturnsAsync(new Platform { Id = req.Platforms[0], Type = "PC" });
        _publisherRepoMock.Setup(x => x.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher { Id = publisherId, CompanyName = "Pub" });

        var result = await _gameService.CreateGameAsync(req);

        Assert.NotNull(result);
        Assert.Equal("Test Game", result.Name);
        Assert.Equal("test-game", result.Key);
        _gameRepoMock.Verify(x => x.AddAsync(It.IsAny<Game>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGameByKeyAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("invalid")).ReturnsAsync((Game?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _gameService.GetGameByKeyAsync("invalid"));
    }

    [Fact]
    public async Task GetGameByKeyAsyncReturnsGameWhenExists()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("valid")).ReturnsAsync(new Game { Name = "Valid", Key = "valid" });
        var result = await _gameService.GetGameByKeyAsync("valid");
        Assert.Equal("Valid", result.Name);
    }

    [Fact]
    public async Task GetAllGamesAsyncReturnsGames()
    {
        _gameRepoMock.Setup(x => x.GetAllWithDetailsAsync()).ReturnsAsync([new Game { Name = "G1", Key = "g1" }, new Game { Name = "G2", Key = "g2" }]);
        var result = await _gameService.GetAllGamesAsync();
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGameByIdAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _gameRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _gameService.GetGameByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetGameByIdAsyncReturnsGameWhenExists()
    {
        var id = Guid.NewGuid();
        _gameRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new Game { Id = id, Name = "Valid", Key = "valid" });
        var result = await _gameService.GetGameByIdAsync(id);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetGamesByGenreIdAsyncReturnsGames()
    {
        var genreId = Guid.NewGuid();
        _gameRepoMock.Setup(x => x.GetByGenreIdAsync(genreId)).ReturnsAsync([new Game { Name = "G1", Key = "g1" }]);
        var result = await _gameService.GetGamesByGenreIdAsync(genreId);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetGamesByPlatformIdAsyncReturnsGames()
    {
        var platformId = Guid.NewGuid();
        _gameRepoMock.Setup(x => x.GetByPlatformIdAsync(platformId)).ReturnsAsync([new Game { Name = "G2", Key = "g2" }]);
        var result = await _gameService.GetGamesByPlatformIdAsync(platformId);
        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateGameAsyncUpdatesWhenValidRequest()
    {
        var gameId = Guid.NewGuid();
        var publisherId = Guid.NewGuid();
        var req = new UpdateGameRequest
        {
            Game = new UpdateGameBody { Id = gameId, Name = "Updated", Key = "updated-key", Description = "new desc", UnitInStock = 1 },
            VendorOffers =
            [
                new GameVendorOfferRequest { Vendor = "Store", PurchaseUrl = "https://example.com", Price = 10, ReferencePrice = 12 },
            ],
            Genres = [Guid.NewGuid()],
            Platforms = [],
            Publisher = publisherId,
        };

        var existingGame = new Game { Id = gameId, Name = "Old", Key = "old" };
        _gameRepoMock.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(existingGame);
        _gameRepoMock.Setup(x => x.GetByKeyAsync(req.Game.Key)).ReturnsAsync((Game?)null);
        _genreRepoMock.Setup(x => x.GetByIdAsync(req.Genres[0])).ReturnsAsync(new Genre { Id = req.Genres[0], Name = "RPG" });
        _publisherRepoMock.Setup(x => x.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher { Id = publisherId, CompanyName = "Pub" });
        _gameVendorOfferRepoMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GameVendorOffer, bool>>>()))
            .ReturnsAsync([]);

        await _gameService.UpdateGameAsync(req);

        Assert.Equal("Updated", existingGame.Name);
        Assert.Equal("updated-key", existingGame.Key);
        Assert.Single(existingGame.Genres);
        _gameRepoMock.Verify(x => x.Update(existingGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsyncDeletesWhenExists()
    {
        var existingGame = new Game { Name = "ToDelete", Key = "to-delete" };
        _gameRepoMock.Setup(x => x.GetByKeyAsync("to-delete")).ReturnsAsync(existingGame);

        await _gameService.DeleteGameAsync("to-delete");

        _gameRepoMock.Verify(x => x.Delete(existingGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("not-found")).ReturnsAsync((Game?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _gameService.DeleteGameAsync("not-found"));
    }

    [Fact]
    public async Task DownloadGameFileAsyncReturnsFileResponseWhenExists()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("test")).ReturnsAsync(new Game { Name = "Test", Key = "test", Description = "file desc" });

        var result = await _gameService.DownloadGameFileAsync("test");

        Assert.NotNull(result);
        Assert.Contains("Test", System.Text.Encoding.UTF8.GetString(result.Content));
        Assert.Contains("test", System.Text.Encoding.UTF8.GetString(result.Content));
        Assert.StartsWith("Test_", result.FileName);
    }

    [Fact]
    public async Task DownloadGameFileAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("not-found")).ReturnsAsync((Game?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _gameService.DownloadGameFileAsync("not-found"));
    }

    [Fact]
    public async Task UpdateGameAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        var req = new UpdateGameRequest
        {
            Game = new UpdateGameBody { Id = Guid.NewGuid(), Name = "Update", Key = "up", Description = "desc", UnitInStock = 1 },
            VendorOffers =
            [
                new GameVendorOfferRequest { Vendor = "Store", PurchaseUrl = "https://example.com", Price = 1, ReferencePrice = 1 },
            ],
            Publisher = Guid.NewGuid(),
        };
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _gameService.UpdateGameAsync(req));
    }

    [Fact]
    public void GetPaginationOptionsReturnsCorrectOptions()
    {
        var result = _gameService.GetPaginationOptions();

        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains("10", result);
        Assert.Contains("20", result);
        Assert.Contains("50", result);
        Assert.Contains("100", result);
        Assert.Contains("all", result);
    }

    [Fact]
    public void GetSortingOptionsReturnsCorrectOptions()
    {
        var result = _gameService.GetSortingOptions();

        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains("Most popular", result);
        Assert.Contains("Most commented", result);
        Assert.Contains("Price ASC", result);
        Assert.Contains("Price DESC", result);
        Assert.Contains("New", result);
    }

    [Fact]
    public void GetPublishDateFilterOptionsReturnsCorrectOptions()
    {
        var result = _gameService.GetPublishDateFilterOptions();

        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains("last week", result);
        Assert.Contains("last month", result);
        Assert.Contains("last year", result);
        Assert.Contains("2 years", result);
        Assert.Contains("3 years", result);
    }

    [Fact]
    public async Task IncrementGameViewCountAsyncIncrementsCount()
    {
        var gameId = Guid.NewGuid();
        _gameRepoMock.Setup(x => x.IncrementViewCountAsync(gameId)).Returns(Task.CompletedTask);

        await _gameService.IncrementGameViewCountAsync(gameId);

        _gameRepoMock.Verify(x => x.IncrementViewCountAsync(gameId), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGamesWithFiltersAsyncAppliesFiltersAndReturnsPagedResult()
    {
        var options = new DbContextOptionsBuilder<GamestoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new GamestoreDbContext(options);
        var publisherId = Guid.NewGuid();
        context.Publishers.Add(new Publisher { Id = publisherId, CompanyName = "Pub" });
        context.Games.AddRange(
            new Game { Id = Guid.NewGuid(), Name = "Game 1", Key = "game-1", ViewCount = 5, PublishDate = DateTime.UtcNow.AddDays(-1), PublisherId = publisherId, Comments = [] },
            new Game { Id = Guid.NewGuid(), Name = "Game 2", Key = "game-2", ViewCount = 10, PublishDate = DateTime.UtcNow.AddMonths(-2), PublisherId = publisherId, Comments = [] });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var unitOfWork = new UnitOfWork(context);
        var service = new GameService(unitOfWork, _loggerMock.Object);
        var request = new GameFilterRequest
        {
            PageNumber = 1,
            PageSize = "10",
        };

        var result = await service.GetGamesWithFiltersAsync(request);
        var games = result.Games.ToList();

        Assert.NotNull(result);
        Assert.NotNull(result.Games);
        Assert.Equal(2, games.Count);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.TotalPages);
    }
}
