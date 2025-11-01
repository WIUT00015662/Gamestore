using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class PlatformServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPlatformRepository> _platformRepoMock;
    private readonly Mock<ILogger<PlatformService>> _loggerMock;
    private readonly PlatformService _platformService;

    public PlatformServiceTests()
    {
        _platformRepoMock = new Mock<IPlatformRepository>();
        _loggerMock = new Mock<ILogger<PlatformService>>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Platforms).Returns(_platformRepoMock.Object);

        _platformService = new PlatformService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePlatformAsyncThrowsEntityAlreadyExistsExceptionWhenTypeExists()
    {
        var req = new CreatePlatformRequest { Platform = new CreatePlatformBody { Type = "PC" } };
        _platformRepoMock.Setup(x => x.GetByTypeAsync("PC")).ReturnsAsync(new Platform { Type = "PC" });

        await Assert.ThrowsAsync<EntityAlreadyExistsException>(() => _platformService.CreatePlatformAsync(req));
    }

    [Fact]
    public async Task CreatePlatformAsyncCreatesPlatformAndReturnsResponse()
    {
        var req = new CreatePlatformRequest { Platform = new CreatePlatformBody { Type = "PC" } };
        _platformRepoMock.Setup(x => x.GetByTypeAsync("PC")).ReturnsAsync((Platform?)null);

        var result = await _platformService.CreatePlatformAsync(req);

        Assert.NotNull(result);
        Assert.Equal("PC", result.Type);
        _platformRepoMock.Verify(x => x.AddAsync(It.IsAny<Platform>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPlatformByIdAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _platformRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Platform?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _platformService.GetPlatformByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPlatformByIdAsyncReturnsPlatformWhenExists()
    {
        var id = Guid.NewGuid();
        _platformRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new Platform { Id = id, Type = "Valid" });
        var result = await _platformService.GetPlatformByIdAsync(id);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetAllPlatformsAsyncReturnsPlatforms()
    {
        _platformRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync([new Platform { Type = "P1" }, new Platform { Type = "P2" }]);
        var result = await _platformService.GetAllPlatformsAsync();
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPlatformsByGameKeyAsyncReturnsPlatforms()
    {
        _platformRepoMock.Setup(x => x.GetByGameKeyAsync("game")).ReturnsAsync([new Platform { Type = "P1" }]);
        var result = await _platformService.GetPlatformsByGameKeyAsync("game");
        Assert.Single(result);
    }

    [Fact]
    public async Task UpdatePlatformAsyncUpdatesWhenValidRequest()
    {
        var platformId = Guid.NewGuid();
        var req = new UpdatePlatformRequest { Platform = new UpdatePlatformBody { Id = platformId, Type = "Updated" } };

        var existingPlatform = new Platform { Id = platformId, Type = "Old" };
        _platformRepoMock.Setup(x => x.GetByIdAsync(platformId)).ReturnsAsync(existingPlatform);

        await _platformService.UpdatePlatformAsync(req);

        Assert.Equal("Updated", existingPlatform.Type);
        _platformRepoMock.Verify(x => x.Update(existingPlatform), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformAsyncDeletesWhenExists()
    {
        var platformId = Guid.NewGuid();
        var existingPlatform = new Platform { Id = platformId, Type = "ToDelete" };
        _platformRepoMock.Setup(x => x.GetByIdAsync(platformId)).ReturnsAsync(existingPlatform);

        await _platformService.DeletePlatformAsync(platformId);

        _platformRepoMock.Verify(x => x.Delete(existingPlatform), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _platformRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Platform?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _platformService.DeletePlatformAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdatePlatformAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        var req = new UpdatePlatformRequest { Platform = new UpdatePlatformBody { Id = Guid.NewGuid(), Type = "Update" } };
        _platformRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Platform?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _platformService.UpdatePlatformAsync(req));
    }
}
