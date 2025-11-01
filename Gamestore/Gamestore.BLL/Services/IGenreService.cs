using Gamestore.BLL.DTOs.Genre;

namespace Gamestore.BLL.Services;

public interface IGenreService
{
    Task<GenreResponse> CreateGenreAsync(CreateGenreRequest request);

    Task<GenreResponse> GetGenreByIdAsync(Guid id);

    Task<IEnumerable<GenreResponse>> GetAllGenresAsync();

    Task<IEnumerable<GenreResponse>> GetGenresByParentIdAsync(Guid parentId);

    Task<IEnumerable<GenreResponse>> GetGenresByGameKeyAsync(string gameKey);

    Task UpdateGenreAsync(UpdateGenreRequest request);

    Task DeleteGenreAsync(Guid id);
}
