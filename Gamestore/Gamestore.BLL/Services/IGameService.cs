using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Publisher;

namespace Gamestore.BLL.Services;

public interface IGameService
{
    Task<GameResponse> CreateGameAsync(CreateGameRequest request);

    Task<GameResponse> GetGameByKeyAsync(string key);

    Task<GameResponse> GetGameByIdAsync(Guid id);

    Task<IEnumerable<GameResponse>> GetAllGamesAsync();

    Task<int> GetGamesCountAsync();

    Task<IEnumerable<GameResponse>> GetGamesByGenreIdAsync(Guid genreId);

    Task<IEnumerable<GameResponse>> GetGamesByPlatformIdAsync(Guid platformId);

    Task<IEnumerable<GameResponse>> GetGamesByPublisherNameAsync(string companyName);

    Task<PublisherResponse> GetPublisherByGameKeyAsync(string key);

    Task<GetGamesResponse> GetGamesWithFiltersAsync(GameFilterRequest request);

    List<string> GetPaginationOptions();

    List<string> GetSortingOptions();

    List<string> GetPublishDateFilterOptions();

    Task IncrementGameViewCountAsync(Guid gameId);

    Task UpdateGameAsync(UpdateGameRequest request);

    Task DeleteGameAsync(string key);

    Task<GameFileResponse> DownloadGameFileAsync(string key);
}
