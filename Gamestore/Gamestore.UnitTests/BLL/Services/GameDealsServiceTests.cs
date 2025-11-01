using Gamestore.BLL.DTOs.Deals;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class GameDealsServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRepository<GameVendorOffer>> _gameVendorOfferRepositoryMock;
    private readonly Mock<IRepository<GameDiscountSnapshot>> _gameDiscountSnapshotRepositoryMock;
    private readonly Mock<IDiscountSimulationService> _discountSimulationServiceMock;
    private readonly Mock<IDiscountNotificationService> _discountNotificationServiceMock;
    private readonly GameDealsService _service;

    public GameDealsServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _gameRepositoryMock = new Mock<IGameRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _gameVendorOfferRepositoryMock = new Mock<IRepository<GameVendorOffer>>();
        _gameDiscountSnapshotRepositoryMock = new Mock<IRepository<GameDiscountSnapshot>>();
        _discountSimulationServiceMock = new Mock<IDiscountSimulationService>();
        _discountNotificationServiceMock = new Mock<IDiscountNotificationService>();

        _unitOfWorkMock.Setup(x => x.Games).Returns(_gameRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.GameVendorOffers).Returns(_gameVendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.GameDiscountSnapshots).Returns(_gameDiscountSnapshotRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _service = new GameDealsService(
            _unitOfWorkMock.Object,
            _discountSimulationServiceMock.Object,
            _discountNotificationServiceMock.Object);
    }

    [Fact]
    public async Task PollDiscountsAsyncSavesOnlySendWorthyDiscountsAndNotifiesUsers()
    {
        var gameId = Guid.NewGuid();
        var offers = new List<GameVendorOffer>
        {
            new() { Id = Guid.NewGuid(), GameId = gameId, GameName = "Game 1", Vendor = "V1", PurchaseUrl = "https://v1/1", Price = 100m },
            new() { Id = Guid.NewGuid(), GameId = gameId, GameName = "Game 1", Vendor = "V2", PurchaseUrl = "https://v2/1", Price = 120m },
            new() { Id = Guid.NewGuid(), GameId = gameId, GameName = "Game 1", Vendor = "V3", PurchaseUrl = "https://v3/1", Price = 80m },
        };

        var savedSnapshots = new List<GameDiscountSnapshot>();

        _gameVendorOfferRepositoryMock.Setup(x => x.GetCountAsync()).ReturnsAsync(offers.Count);
        _gameVendorOfferRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(offers);
        _gameDiscountSnapshotRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<GameDiscountSnapshot>()))
            .Callback<GameDiscountSnapshot>(savedSnapshots.Add)
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new User { Id = Guid.NewGuid(), Name = "john", PasswordHash = "hash" },
        ]);

        _discountSimulationServiceMock.SetupSequence(x => x.GenerateDiscountPercent())
            .Returns(20m)
            .Returns(10m)
            .Returns(40m);

        var result = await _service.PollDiscountsAsync();

        Assert.Equal(2, result.TotalDiscountedGames);
        Assert.Equal(2, result.FeaturedGamesCount);
        Assert.All(savedSnapshots, snapshot => Assert.True(snapshot.DiscountPercent >= 20m));
        _discountNotificationServiceMock.Verify(x => x.NotifyUsersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<DiscountedGameResponse>>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestFeaturedDiscountsAsyncReturnsLatestPollingRunOnly()
    {
        var oldRunId = Guid.NewGuid();
        var newRunId = Guid.NewGuid();
        var oldTime = DateTime.UtcNow.AddMinutes(-20);
        var newTime = DateTime.UtcNow;

        _gameDiscountSnapshotRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new GameDiscountSnapshot
            {
                Id = Guid.NewGuid(),
                PollingRunId = oldRunId,
                PolledAt = oldTime,
                GameId = Guid.NewGuid(),
                GameName = "Old",
                Vendor = "V1",
                PurchaseUrl = "https://old",
                OriginalPrice = 100m,
                DiscountedPrice = 70m,
                DiscountPercent = 30m,
                IsFeatured = true,
            },
            new GameDiscountSnapshot
            {
                Id = Guid.NewGuid(),
                PollingRunId = newRunId,
                PolledAt = newTime,
                GameId = Guid.NewGuid(),
                GameName = "New",
                Vendor = "V2",
                PurchaseUrl = "https://new",
                OriginalPrice = 100m,
                DiscountedPrice = 60m,
                DiscountPercent = 40m,
                IsFeatured = true,
            },
        ]);

        var result = await _service.GetLatestFeaturedDiscountsAsync();

        var item = Assert.Single(result);
        Assert.Equal("New", item.GameName);
    }

    [Fact]
    public async Task GetOffersByGameKeyAsyncReturnsAllOffersForGame()
    {
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Name = "Game", Key = "game" };

        _gameRepositoryMock.Setup(x => x.GetByKeyIncludingDeletedAsync("game")).ReturnsAsync(game);
        _gameVendorOfferRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GameVendorOffer, bool>>>())).ReturnsAsync([
            new GameVendorOffer { Id = Guid.NewGuid(), GameId = gameId, GameName = "Game", Vendor = "V1", PurchaseUrl = "https://v1", Price = 10m },
            new GameVendorOffer { Id = Guid.NewGuid(), GameId = gameId, GameName = "Game", Vendor = "V2", PurchaseUrl = "https://v2", Price = 20m },
        ]);

        var result = await _service.GetOffersByGameKeyAsync("game");

        Assert.Equal(2, result.Count);
    }
}
