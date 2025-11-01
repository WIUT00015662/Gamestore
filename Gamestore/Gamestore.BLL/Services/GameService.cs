using System.Globalization;
using System.Text;
using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Publisher;
using Gamestore.BLL.Filters;
using Gamestore.BLL.Filters.Pipes;
using Gamestore.BLL.Mapping;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

/// <summary>
/// Game service implementation.
/// </summary>
public class GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger) : IGameService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GameService> _logger = logger;

    /// <inheritdoc/>
    public async Task<GameResponse> CreateGameAsync(CreateGameRequest request)
    {
        _logger.LogInformation("Creating game: {Name}", request.Game.Name);

        var key = request.Game.Key;
        var existingGame = await _unitOfWork.Games.GetByKeyAsync(key);
        if (existingGame is not null)
        {
            throw new EntityAlreadyExistsException(nameof(Game), nameof(Game.Key), key);
        }

        var publisher = await _unitOfWork.Publishers.GetByIdAsync(request.Publisher)
            ?? throw new EntityNotFoundException(nameof(Publisher), request.Publisher);

        var genres = await LoadGenresAsync(request.Genres);
        var platforms = await LoadPlatformsAsync(request.Platforms);

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Name = request.Game.Name,
            Key = key,
            Description = request.Game.Description,
            Price = request.Game.Price,
            UnitInStock = request.Game.UnitInStock,
            Discount = request.Game.Discount,
            PublisherId = publisher.Id,
            Publisher = publisher,
        };

        foreach (var genre in genres)
        {
            game.Genres.Add(genre);
        }

        foreach (var platform in platforms)
        {
            game.Platforms.Add(platform);
        }

        await _unitOfWork.Games.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return game.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<GameResponse> GetGameByKeyAsync(string key)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Game), key);

        return game.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<GameResponse> GetGameByIdAsync(Guid id)
    {
        var game = await _unitOfWork.Games.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Game), id);

        return game.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameResponse>> GetAllGamesAsync(bool includeDeleted = false)
    {
        var games = includeDeleted
            ? await _unitOfWork.Games.GetAllWithDetailsIncludingDeletedAsync()
            : await _unitOfWork.Games.GetAllAsync();

        return games.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<int> GetGamesCountAsync()
    {
        return await _unitOfWork.Games.GetCountAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameResponse>> GetGamesByGenreIdAsync(Guid genreId)
    {
        var games = await _unitOfWork.Games.GetByGenreIdAsync(genreId);
        return games.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameResponse>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        var games = await _unitOfWork.Games.GetByPlatformIdAsync(platformId);
        return games.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameResponse>> GetGamesByPublisherNameAsync(string companyName)
    {
        var games = await _unitOfWork.Games.GetByPublisherNameAsync(companyName);
        return games.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<PublisherResponse> GetPublisherByGameKeyAsync(string key)
    {
        var publisher = await _unitOfWork.Publishers.GetByGameKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Publisher), key);

        return publisher.ToResponse();
    }

    /// <inheritdoc/>
    public async Task UpdateGameAsync(UpdateGameRequest request, bool includeDeleted = false)
    {
        _logger.LogInformation("Updating game with ID: {Id}", request.Game.Id);

        var game = (includeDeleted
            ? await _unitOfWork.Games.GetByIdWithDetailsIncludingDeletedAsync(request.Game.Id)
            : await _unitOfWork.Games.GetByIdWithDetailsAsync(request.Game.Id)) ?? throw new EntityNotFoundException(nameof(Game), request.Game.Id);
        var existingByKey = includeDeleted
            ? await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(request.Game.Key)
            : await _unitOfWork.Games.GetByKeyAsync(request.Game.Key);

        if (existingByKey is not null && existingByKey.Id != request.Game.Id)
        {
            throw new EntityAlreadyExistsException(nameof(Game), nameof(Game.Key), request.Game.Key);
        }

        var publisher = await _unitOfWork.Publishers.GetByIdAsync(request.Publisher)
            ?? throw new EntityNotFoundException(nameof(Publisher), request.Publisher);

        var genres = await LoadGenresAsync(request.Genres);
        var platforms = await LoadPlatformsAsync(request.Platforms);

        game.Name = request.Game.Name;
        game.Key = request.Game.Key;
        game.Description = request.Game.Description;
        game.Price = request.Game.Price;
        game.UnitInStock = request.Game.UnitInStock;
        game.Discount = request.Game.Discount;
        game.PublisherId = publisher.Id;
        game.Publisher = publisher;

        game.Genres.Clear();
        foreach (var genre in genres)
        {
            game.Genres.Add(genre);
        }

        game.Platforms.Clear();
        foreach (var platform in platforms)
        {
            game.Platforms.Add(platform);
        }

        _unitOfWork.Games.Update(game);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteGameAsync(string key)
    {
        _logger.LogInformation("Deleting game with key: {Key}", key);
        var game = await _unitOfWork.Games.GetByKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Game), key);

        game.IsDeleted = true;
        _unitOfWork.Games.Update(game);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<GameFileResponse> DownloadGameFileAsync(string key)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Game), key);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var content = $"Game: {game.Name}\nKey: {game.Key}\nDescription: {game.Description}\nGenerated: {timestamp}";

        return new GameFileResponse
        {
            Content = Encoding.UTF8.GetBytes(content),
            FileName = $"{game.Name}_{timestamp}.txt",
        };
    }

    /// <inheritdoc/>
    public async Task<GetGamesResponse> GetGamesWithFiltersAsync(GameFilterRequest request)
    {
        _logger.LogInformation("Getting games with filters: Page {Page}, Size {Size}", request.PageNumber, request.PageSize);

        var query = _unitOfWork.Games.GetQueryable();
        var pageSize = GetPageSize(request.PageSize);
        var countPipeline = BuildFilterPipeline(request, includePagination: false);
        var pagingPipeline = BuildFilterPipeline(request, includePagination: true, pageSize);

        var totalCount = await countPipeline.Execute(query).CountAsync();
        var games = await pagingPipeline.Execute(query).ToListAsync();
        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;

        return new GetGamesResponse
        {
            Games = [.. games.ToResponse()],
            TotalPages = totalPages,
            CurrentPage = request.PageNumber,
        };
    }

    /// <inheritdoc/>
    public List<string> GetPaginationOptions()
    {
        return ["10", "20", "50", "100", "all"];
    }

    /// <inheritdoc/>
    public List<string> GetSortingOptions()
    {
        return [
            "Most popular",
            "Most commented",
            "Price ASC",
            "Price DESC",
            "New",
        ];
    }

    /// <inheritdoc/>
    public List<string> GetPublishDateFilterOptions()
    {
        return [
            "last week",
            "last month",
            "last year",
            "2 years",
            "3 years",
        ];
    }

    /// <inheritdoc/>
    public async Task IncrementGameViewCountAsync(Guid gameId)
    {
        _logger.LogInformation("Incrementing view count for game ID: {GameId}", gameId);
        await _unitOfWork.Games.IncrementViewCountAsync(gameId);
        await _unitOfWork.SaveChangesAsync();
    }

    private static FilterPipeline BuildFilterPipeline(GameFilterRequest request, bool includePagination, int pageSize = 0)
    {
        var pipeline = new FilterPipeline();

        // Add filter pipes - order matters for performance
        pipeline.AddPipe(new GenreFilterPipe(request.GenreIds));
        pipeline.AddPipe(new PlatformFilterPipe(request.PlatformIds));
        pipeline.AddPipe(new PublisherFilterPipe(request.PublisherIds));
        pipeline.AddPipe(new PriceFilterPipe(request.MinPrice, request.MaxPrice));
        pipeline.AddPipe(new PublishDateFilterPipe(request.PublishDateFilter));
        pipeline.AddPipe(new GameNameFilterPipe(request.GameName));
        pipeline.AddPipe(new SortingPipe(request.SortBy));

        // Add pagination pipe last
        if (includePagination && pageSize > 0)
        {
            pipeline.AddPipe(new PaginationPipe(request.PageNumber, pageSize));
        }

        return pipeline;
    }

    private static int GetPageSize(string? pageSizeStr) =>
        string.IsNullOrEmpty(pageSizeStr) || pageSizeStr.Equals("all", StringComparison.OrdinalIgnoreCase) ? int.MaxValue : (int.TryParse(pageSizeStr, out var size) ? size : 10);

    private async Task<List<Genre>> LoadGenresAsync(IEnumerable<Guid> genreIds)
    {
        var genres = new List<Genre>();
        foreach (var genreId in genreIds.Distinct())
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreId)
                ?? throw new EntityNotFoundException(nameof(Genre), genreId);
            genres.Add(genre);
        }

        return genres;
    }

    private async Task<List<Platform>> LoadPlatformsAsync(IEnumerable<Guid> platformIds)
    {
        var platforms = new List<Platform>();
        foreach (var platformId in platformIds.Distinct())
        {
            var platform = await _unitOfWork.Platforms.GetByIdAsync(platformId)
                ?? throw new EntityNotFoundException(nameof(Platform), platformId);
            platforms.Add(platform);
        }

        return platforms;
    }
}
