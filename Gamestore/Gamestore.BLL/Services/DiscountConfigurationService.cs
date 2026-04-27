using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public interface IDiscountConfigurationService
{
    Task<DiscountConfiguration> GetActiveConfigurationAsync();

    Task EnsureDefaultConfigurationAsync();
}

public class DiscountConfigurationService(IUnitOfWork unitOfWork) : IDiscountConfigurationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<DiscountConfiguration> GetActiveConfigurationAsync()
    {
        var config = await _unitOfWork.DiscountConfigurations.GetActiveConfigurationAsync();
        if (config == null)
        {
            await EnsureDefaultConfigurationAsync();
            config = await _unitOfWork.DiscountConfigurations.GetActiveConfigurationAsync();
        }

        return config ?? throw new InvalidOperationException("Failed to create discount configuration");
    }

    public async Task EnsureDefaultConfigurationAsync()
    {
        var existing = await _unitOfWork.DiscountConfigurations.GetActiveConfigurationAsync();
        if (existing != null)
        {
            return;
        }

        var config = new DiscountConfiguration
        {
            Id = Guid.NewGuid(),
            DiscountProbability = 0.3m,
            DiscountPercentageMin = 5m,
            DiscountPercentageMax = 50m,
            TimeWindowMinutes = 30,
            DiscountRevertProbability = 0.4m,
            IsActive = true,
        };

        await _unitOfWork.DiscountConfigurations.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
    }
}
