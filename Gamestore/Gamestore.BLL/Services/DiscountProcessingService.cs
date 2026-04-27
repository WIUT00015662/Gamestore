using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public interface IDiscountProcessingService
{
    Task<PollingRun> ProcessDiscountsAsync(DiscountConfiguration config);
}

public class DiscountProcessingService(
    IUnitOfWork unitOfWork,
    IDiscountSimulationService simulationService,
    ILogger<DiscountProcessingService> logger) : IDiscountProcessingService
{
    private static readonly SemaphoreSlim PollLock = new(1, 1);
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDiscountSimulationService _simulationService = simulationService;
    private readonly ILogger<DiscountProcessingService> _logger = logger;

    public async Task<PollingRun> ProcessDiscountsAsync(DiscountConfiguration config)
    {
        await PollLock.WaitAsync();
        var pollingRun = new PollingRun
        {
            Id = Guid.NewGuid(),
            RunAt = DateTime.UtcNow,
            Status = "Running",
        };

        try
        {
            await _unitOfWork.PollingRuns.AddAsync(pollingRun);

            // Step 1: Process existing discounts - revert or keep
            var revertedOfferIds = await ProcessExistingDiscountsAsync(config, pollingRun);

            // Step 2: Process new discounts - candidates excluding reverted
            await ProcessNewDiscountsAsync(config, pollingRun, revertedOfferIds);

            // Step 3: Update current prices
            await UpdateCurrentPricesAsync(pollingRun);

            // Step 4: Identify active discounts and create snapshots
            await CreateDiscountSnapshotsAsync(pollingRun);

            pollingRun.Status = "Completed";
            pollingRun.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Polling run {RunId} completed: {ProcessedCount} offers, {NewCount} new discounts, {RevertedCount} reverted",
                pollingRun.Id, pollingRun.ProcessedOffersCount, pollingRun.NewDiscountsCreated, pollingRun.DiscountsReverted);
        }
        catch (Exception ex)
        {
            pollingRun.Status = "Failed";
            pollingRun.ErrorMessage = ex.Message;
            pollingRun.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Polling run {RunId} failed", pollingRun.Id);
        }
        finally
        {
            _unitOfWork.PollingRuns.Update(pollingRun);
            await _unitOfWork.SaveChangesAsync();
            PollLock.Release();
        }

        return pollingRun;
    }

    private async Task<HashSet<Guid>> ProcessExistingDiscountsAsync(DiscountConfiguration config, PollingRun pollingRun)
    {
        var activeDiscounts = await _unitOfWork.GameDiscounts.GetActiveDiscountsAsync();
        var revertedOfferIds = new HashSet<Guid>();

        foreach (var discount in activeDiscounts)
        {
            if (_simulationService.ShouldRevertDiscount(config.DiscountRevertProbability))
            {
                discount.IsCurrentlyActive = false;
                discount.RevertedAt = DateTime.UtcNow;
                _unitOfWork.GameDiscounts.Update(discount);
                var offer = await _unitOfWork.GameVendorOffers.GetByIdAsync(discount.GameVendorOfferId);
                if (offer != null)
                {
                    revertedOfferIds.Add(offer.Id);
                    offer.CurrentPrice = offer.TruePrice;
                    _unitOfWork.GameVendorOffers.Update(offer);
                }
                pollingRun.DiscountsReverted++;
            }
        }

        return revertedOfferIds;
    }

    private async Task ProcessNewDiscountsAsync(DiscountConfiguration config, PollingRun pollingRun, HashSet<Guid> revertedOfferIds)
    {
        var allOffers = await _unitOfWork.GameVendorOffers.GetAllAsync();

        foreach (var offer in allOffers)
        {
            if (revertedOfferIds.Contains(offer.Id))
            {
                continue;
            }

            // Check if offer already has active discount
            var existingDiscounts = await _unitOfWork.GameDiscounts.GetDiscountsByOfferIdAsync(offer.Id);
            if (existingDiscounts.Any(d => d.IsCurrentlyActive))
            {
                continue;  // Skip offers with active discounts
            }

            // Apply new discount based on probability
            if (_simulationService.ShouldApplyDiscount(config.DiscountProbability))
            {
                var discountPercent = _simulationService.GenerateDiscountPercent(
                    config.DiscountPercentageMin,
                    config.DiscountPercentageMax);

                var discount = new GameDiscount
                {
                    Id = Guid.NewGuid(),
                    GameVendorOfferId = offer.Id,
                    DiscountPercent = discountPercent,
                    IsCurrentlyActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.GameDiscounts.AddAsync(discount);

                // Update current price
                var discountedPrice = Math.Round(
                    offer.TruePrice * (1 - (discountPercent / 100m)),
                    2,
                    MidpointRounding.AwayFromZero);

                offer.CurrentPrice = discountedPrice;
                _unitOfWork.GameVendorOffers.Update(offer);

                pollingRun.NewDiscountsCreated++;
            }
        }
    }

    private async Task UpdateCurrentPricesAsync(PollingRun pollingRun)
    {
        var offers = await _unitOfWork.GameVendorOffers.GetAllAsync();
        pollingRun.ProcessedOffersCount = offers.Count();

        foreach (var offer in offers)
        {
            offer.LastPolledPrice = offer.CurrentPrice;
            offer.LastPolledAt = pollingRun.RunAt;
            _unitOfWork.GameVendorOffers.Update(offer);
        }
    }

    private async Task CreateDiscountSnapshotsAsync(PollingRun pollingRun)
    {
        var activeDiscounts = await _unitOfWork.GameDiscounts.GetActiveDiscountsAsync();

        foreach (var discount in activeDiscounts)
        {
            var offer = await _unitOfWork.GameVendorOffers.GetByIdAsync(discount.GameVendorOfferId);
            if (offer == null)
            {
                continue;
            }

            var snapshot = new GameDiscountSnapshot
            {
                Id = Guid.NewGuid(),
                PollingRunId = pollingRun.Id,
                PolledAt = DateTime.UtcNow,
                GameId = offer.GameId,
                GameName = offer.GameName,
                Vendor = offer.Vendor,
                PurchaseUrl = offer.PurchaseUrl,
                OriginalPrice = offer.TruePrice,
                DiscountedPrice = offer.CurrentPrice,
                DiscountPercent = discount.DiscountPercent,
                IsNewDiscount = discount.CreatedAt >= pollingRun.RunAt,
            };

            await _unitOfWork.GameDiscountSnapshots.AddAsync(snapshot);
        }

        // Mark top 5 by discount percentage as featured
        var snapshots = (await _unitOfWork.GameDiscountSnapshots.FindAsync(
            x => x.PollingRunId == pollingRun.Id)).ToList();

        var featuredIds = snapshots
            .GroupBy(x => x.GameId)
            .Select(g => g.OrderByDescending(x => x.DiscountPercent).First())
            .OrderByDescending(x => x.DiscountPercent)
            .Take(5)
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var snapshot in snapshots)
        {
            if (featuredIds.Contains(snapshot.Id))
            {
                snapshot.IsFeatured = true;
                _unitOfWork.GameDiscountSnapshots.Update(snapshot);
            }
        }
    }
}
