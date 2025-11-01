using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.Mapping;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

/// <summary>
/// Genre service implementation.
/// </summary>
public class GenreService(IUnitOfWork unitOfWork, ILogger<GenreService> logger) : IGenreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GenreService> _logger = logger;

    /// <inheritdoc/>
    public async Task<GenreResponse> CreateGenreAsync(CreateGenreRequest request)
    {
        _logger.LogInformation("Creating genre with name: {Name}", request.Genre.Name);
        var existingGenre = await _unitOfWork.Genres.GetByNameAsync(request.Genre.Name);
        if (existingGenre is not null)
        {
            throw new EntityAlreadyExistsException(nameof(Genre), nameof(Genre.Name), request.Genre.Name);
        }

        if (request.Genre.ParentGenreId.HasValue)
        {
            _ = await _unitOfWork.Genres.GetByIdAsync(request.Genre.ParentGenreId.Value)
                ?? throw new EntityNotFoundException(nameof(Genre), request.Genre.ParentGenreId.Value);
        }

        var genre = new Genre
        {
            Id = Guid.NewGuid(),
            Name = request.Genre.Name,
            ParentGenreId = request.Genre.ParentGenreId,
        };

        await _unitOfWork.Genres.AddAsync(genre);
        await _unitOfWork.SaveChangesAsync();

        return genre.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<GenreResponse> GetGenreByIdAsync(Guid id)
    {
        var genre = await _unitOfWork.Genres.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Genre), id);

        return genre.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GenreResponse>> GetAllGenresAsync()
    {
        var genres = await _unitOfWork.Genres.GetAllAsync();
        return genres.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GenreResponse>> GetGenresByParentIdAsync(Guid parentId)
    {
        var genres = await _unitOfWork.Genres.GetByParentIdAsync(parentId);
        return genres.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GenreResponse>> GetGenresByGameKeyAsync(string gameKey)
    {
        var genres = await _unitOfWork.Genres.GetByGameKeyAsync(gameKey);
        return genres.ToResponse();
    }

    /// <inheritdoc/>
    public async Task UpdateGenreAsync(UpdateGenreRequest request)
    {
        _logger.LogInformation("Updating genre with ID: {Id}", request.Genre.Id);
        var genre = await _unitOfWork.Genres.GetByIdAsync(request.Genre.Id)
            ?? throw new EntityNotFoundException(nameof(Genre), request.Genre.Id);

        var existingByName = await _unitOfWork.Genres.GetByNameAsync(request.Genre.Name);
        if (existingByName is not null && existingByName.Id != request.Genre.Id)
        {
            throw new EntityAlreadyExistsException(nameof(Genre), nameof(Genre.Name), request.Genre.Name);
        }

        if (request.Genre.ParentGenreId.HasValue)
        {
            if (request.Genre.ParentGenreId.Value == request.Genre.Id)
            {
                throw new ArgumentException("Cycled genre reference is prohibited.");
            }

            var parent = await _unitOfWork.Genres.GetByIdAsync(request.Genre.ParentGenreId.Value)
                ?? throw new EntityNotFoundException(nameof(Genre), request.Genre.ParentGenreId.Value);

            while (parent.ParentGenreId.HasValue)
            {
                if (parent.ParentGenreId.Value == request.Genre.Id)
                {
                    throw new ArgumentException("Cycled genre reference is prohibited.");
                }

                parent = await _unitOfWork.Genres.GetByIdAsync(parent.ParentGenreId.Value)
                    ?? throw new EntityNotFoundException(nameof(Genre), parent.ParentGenreId.Value);
            }
        }

        genre.Name = request.Genre.Name;
        genre.ParentGenreId = request.Genre.ParentGenreId;

        _unitOfWork.Genres.Update(genre);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteGenreAsync(Guid id)
    {
        _logger.LogInformation("Deleting genre with ID: {Id}", id);
        var genre = await _unitOfWork.Genres.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Genre), id);

        _unitOfWork.Genres.Delete(genre);
        await _unitOfWork.SaveChangesAsync();
    }
}
