using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.Mapping;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

/// <summary>
/// Platform service implementation.
/// </summary>
public class PlatformService(IUnitOfWork unitOfWork, ILogger<PlatformService> logger) : IPlatformService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PlatformService> _logger = logger;

    /// <inheritdoc/>
    public async Task<PlatformResponse> CreatePlatformAsync(CreatePlatformRequest request)
    {
        _logger.LogInformation("Creating platform with type: {Type}", request.Platform.Type);
        var existingPlatform = await _unitOfWork.Platforms.GetByTypeAsync(request.Platform.Type);
        if (existingPlatform is not null)
        {
            throw new EntityAlreadyExistsException(nameof(Platform), nameof(Platform.Type), request.Platform.Type);
        }

        var platform = new Platform
        {
            Id = Guid.NewGuid(),
            Type = request.Platform.Type,
        };

        await _unitOfWork.Platforms.AddAsync(platform);
        await _unitOfWork.SaveChangesAsync();

        return platform.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<PlatformResponse> GetPlatformByIdAsync(Guid id)
    {
        var platform = await _unitOfWork.Platforms.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Platform), id);

        return platform.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PlatformResponse>> GetAllPlatformsAsync()
    {
        var platforms = await _unitOfWork.Platforms.GetAllAsync();
        return platforms.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PlatformResponse>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        var platforms = await _unitOfWork.Platforms.GetByGameKeyAsync(gameKey);
        return platforms.ToResponse();
    }

    /// <inheritdoc/>
    public async Task UpdatePlatformAsync(UpdatePlatformRequest request)
    {
        _logger.LogInformation("Updating platform with ID: {Id}", request.Platform.Id);
        var platform = await _unitOfWork.Platforms.GetByIdAsync(request.Platform.Id)
            ?? throw new EntityNotFoundException(nameof(Platform), request.Platform.Id);

        var existingByType = await _unitOfWork.Platforms.GetByTypeAsync(request.Platform.Type);
        if (existingByType is not null && existingByType.Id != request.Platform.Id)
        {
            throw new EntityAlreadyExistsException(nameof(Platform), nameof(Platform.Type), request.Platform.Type);
        }

        platform.Type = request.Platform.Type;

        _unitOfWork.Platforms.Update(platform);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeletePlatformAsync(Guid id)
    {
        _logger.LogInformation("Deleting platform with ID: {Id}", id);
        var platform = await _unitOfWork.Platforms.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Platform), id);

        _unitOfWork.Platforms.Delete(platform);
        await _unitOfWork.SaveChangesAsync();
    }
}
