using Gamestore.BLL.DTOs.Deals;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public class GameDealsService(
    IUnitOfWork unitOfWork,
    IDiscountProcessingService discountProcessingService,
    IDiscountConfigurationService configurationService) : IGameDealsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDiscountProcessingService _discountProcessingService = discountProcessingService;
    private readonly IDiscountConfigurationService _configurationService = configurationService;

    /// <summary>
    /// Executes the 5-step discount processing workflow.
    /// </summary>
    public async Task<DiscountPollingResultResponse> PollDiscountsAsync()
    {
        await EnsureOffersSeededAsync();

        var config = await _configurationService.GetActiveConfigurationAsync();
        var pollingRun = await _discountProcessingService.ProcessDiscountsAsync(config);

        return new DiscountPollingResultResponse
        {
            PollingRunId = pollingRun.Id,
            PolledAt = pollingRun.RunAt,
            TotalDiscountedGames = pollingRun.NewDiscountsCreated + pollingRun.DiscountsReverted,
            FeaturedGamesCount = (await _unitOfWork.GameDiscountSnapshots.FindAsync(
                x => x.PollingRunId == pollingRun.Id && x.IsFeatured)).Count(),
        };
    }

    public async Task<IReadOnlyCollection<DiscountedGameResponse>> GetLatestFeaturedDiscountsAsync(int take = 5)
    {
        var latestRun = await _unitOfWork.PollingRuns.GetLatestRunAsync();
        if (latestRun == null)
        {
            return [];
        }

        var snapshots = await _unitOfWork.GameDiscountSnapshots.FindAsync(
            x => x.PollingRunId == latestRun.Id && x.IsFeatured);

        return [.. snapshots
            .OrderByDescending(x => x.DiscountPercent)
            .Take(take)
            .Select(ToResponse)];
    }

    public async Task<IReadOnlyCollection<DiscountedGameResponse>> GetAllCurrentDiscountsAsync()
    {
        var latestRun = await _unitOfWork.PollingRuns.GetLatestRunAsync();
        if (latestRun == null)
        {
            return [];
        }

        var snapshots = (await _unitOfWork.GameDiscountSnapshots.FindAsync(
            x => x.PollingRunId == latestRun.Id)).ToList();

        // Group by game and select only the highest discount per game
        var groupedByGame = snapshots
            .GroupBy(x => x.GameId)
            .SelectMany(g => g.OrderByDescending(x => x.DiscountPercent).Take(1))
            .OrderByDescending(x => x.DiscountPercent)
            .Select(ToResponse)
            .ToList();

        return groupedByGame;
    }

    public async Task<IReadOnlyCollection<GameVendorOfferResponse>> GetOffersByGameKeyAsync(string key)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Game), key);

        var offers = await _unitOfWork.GameVendorOffers.FindAsync(x => x.GameId == game.Id);
        return
        [
            .. offers.Select(ToOfferResponse),
        ];
    }

    public async Task<EmailSubscriptionResponse> SubscribeEmailAsync(string email)
    {
        var existing = await _unitOfWork.EmailSubscriptions.GetByEmailAsync(email);
        if (existing != null)
        {
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            _unitOfWork.EmailSubscriptions.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return ToEmailSubscriptionResponse(existing);
        }

        var subscription = new EmailSubscription
        {
            Id = Guid.NewGuid(),
            Email = email,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow,
        };

        await _unitOfWork.EmailSubscriptions.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();
        return ToEmailSubscriptionResponse(subscription);
    }

    public async Task UnsubscribeEmailAsync(string email)
    {
        var subscription = await _unitOfWork.EmailSubscriptions.GetByEmailAsync(email);
        if (subscription != null)
        {
            subscription.IsActive = false;
            subscription.UnsubscribedAt = DateTime.UtcNow;
            _unitOfWork.EmailSubscriptions.Update(subscription);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task EnsureOffersSeededAsync()
    {
        var existingOffers = await _unitOfWork.GameVendorOffers.GetCountAsync();
        if (existingOffers > 0)
        {
            return;
        }
    }

    private static DiscountedGameResponse ToResponse(GameDiscountSnapshot snapshot)
    {
        return new DiscountedGameResponse
        {
            GameId = snapshot.GameId,
            GameName = snapshot.GameName,
            Vendor = snapshot.Vendor,
            PurchaseUrl = snapshot.PurchaseUrl,
            OriginalPrice = snapshot.OriginalPrice,
            DiscountedPrice = snapshot.DiscountedPrice,
            DiscountPercent = snapshot.DiscountPercent,
        };
    }

    private static GameVendorOfferResponse ToOfferResponse(GameVendorOffer offer)
    {
        return new GameVendorOfferResponse
        {
            Id = offer.Id,
            Vendor = offer.Vendor,
            PurchaseUrl = offer.PurchaseUrl,
            Price = offer.Price,
            LastPolledPrice = offer.LastPolledPrice,
            LastPolledAt = offer.LastPolledAt,
        };
    }

    private static EmailSubscriptionResponse ToEmailSubscriptionResponse(EmailSubscription subscription)
    {
        return new EmailSubscriptionResponse
        {
            Id = subscription.Id,
            Email = subscription.Email,
            IsActive = subscription.IsActive,
            SubscribedAt = subscription.SubscribedAt,
        };
    }
}
