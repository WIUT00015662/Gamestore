using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class GenreServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenreRepository> _genreRepoMock;
    private readonly Mock<ILogger<GenreService>> _loggerMock;
    private readonly GenreService _genreService;

    public GenreServiceTests()
    {
        _genreRepoMock = new Mock<IGenreRepository>();
        _loggerMock = new Mock<ILogger<GenreService>>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Genres).Returns(_genreRepoMock.Object);

        _genreService = new GenreService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateGenreAsyncThrowsEntityAlreadyExistsExceptionWhenNameExists()
    {
        var req = new CreateGenreRequest { Genre = new CreateGenreBody { Name = "RPG" } };
        _genreRepoMock.Setup(x => x.GetByNameAsync("RPG")).ReturnsAsync(new Genre { Name = "RPG" });

        await Assert.ThrowsAsync<EntityAlreadyExistsException>(() => _genreService.CreateGenreAsync(req));
    }

    [Fact]
    public async Task CreateGenreAsyncCreatesGenreAndReturnsResponse()
    {
        var req = new CreateGenreRequest { Genre = new CreateGenreBody { Name = "RPG" } };
        _genreRepoMock.Setup(x => x.GetByNameAsync("RPG")).ReturnsAsync((Genre?)null);

        var result = await _genreService.CreateGenreAsync(req);

        Assert.NotNull(result);
        Assert.Equal("RPG", result.Name);
        _genreRepoMock.Verify(x => x.AddAsync(It.IsAny<Genre>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGenreByIdAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _genreRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _genreService.GetGenreByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetGenreByIdAsyncReturnsGenreWhenExists()
    {
        var id = Guid.NewGuid();
        _genreRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new Genre { Id = id, Name = "Valid" });
        var result = await _genreService.GetGenreByIdAsync(id);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetAllGenresAsyncReturnsGenres()
    {
        _genreRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync([new Genre { Name = "G1" }, new Genre { Name = "G2" }]);
        var result = await _genreService.GetAllGenresAsync();
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGenresByParentIdAsyncReturnsGenres()
    {
        var parentId = Guid.NewGuid();
        _genreRepoMock.Setup(x => x.GetByParentIdAsync(parentId)).ReturnsAsync([new Genre { Name = "G1", ParentGenreId = parentId }]);
        var result = await _genreService.GetGenresByParentIdAsync(parentId);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsyncReturnsGenres()
    {
        _genreRepoMock.Setup(x => x.GetByGameKeyAsync("game")).ReturnsAsync([new Genre { Name = "G1" }]);
        var result = await _genreService.GetGenresByGameKeyAsync("game");
        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateGenreAsyncUpdatesWhenValidRequest()
    {
        var genreId = Guid.NewGuid();
        var req = new UpdateGenreRequest { Genre = new UpdateGenreBody { Id = genreId, Name = "Updated" } };

        var existingGenre = new Genre { Id = genreId, Name = "Old" };
        _genreRepoMock.Setup(x => x.GetByIdAsync(genreId)).ReturnsAsync(existingGenre);

        await _genreService.UpdateGenreAsync(req);

        Assert.Equal("Updated", existingGenre.Name);
        _genreRepoMock.Verify(x => x.Update(existingGenre), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsyncDeletesWhenExists()
    {
        var genreId = Guid.NewGuid();
        var existingGenre = new Genre { Id = genreId, Name = "ToDelete" };
        _genreRepoMock.Setup(x => x.GetByIdAsync(genreId)).ReturnsAsync(existingGenre);

        await _genreService.DeleteGenreAsync(genreId);

        _genreRepoMock.Verify(x => x.Delete(existingGenre), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        _genreRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _genreService.DeleteGenreAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateGenreAsyncThrowsEntityNotFoundExceptionWhenNotFound()
    {
        var req = new UpdateGenreRequest { Genre = new UpdateGenreBody { Id = Guid.NewGuid(), Name = "Update" } };
        _genreRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _genreService.UpdateGenreAsync(req));
    }
}
