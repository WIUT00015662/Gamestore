using Gamestore.BLL.DTOs.Platform;

namespace Gamestore.BLL.Services;

public interface IPlatformService
{
    Task<PlatformResponse> CreatePlatformAsync(CreatePlatformRequest request);

    Task<PlatformResponse> GetPlatformByIdAsync(Guid id);

    Task<IEnumerable<PlatformResponse>> GetAllPlatformsAsync();

    Task<IEnumerable<PlatformResponse>> GetPlatformsByGameKeyAsync(string gameKey);

    Task UpdatePlatformAsync(UpdatePlatformRequest request);

    Task DeletePlatformAsync(Guid id);
}
