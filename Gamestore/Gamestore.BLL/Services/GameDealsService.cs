using Gamestore.BLL.DTOs.Deals;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public class GameDealsService(
    IUnitOfWork unitOfWork,
    IDiscountSimulationService discountSimulationService,
    IDiscountNotificationService discountNotificationService) : IGameDealsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDiscountSimulationService _discountSimulationService = discountSimulationService;
    private readonly IDiscountNotificationService _discountNotificationService = discountNotificationService;

    public async Task<DiscountPollingResultResponse> PollDiscountsAsync()
    {
        await EnsureOffersSeededAsync();

        var now = DateTime.UtcNow;
        var pollingRunId = Guid.NewGuid();

        List<GameVendorOffer> offers = [.. await _unitOfWork.GameVendorOffers.GetAllAsync()];
        var snapshots = new List<GameDiscountSnapshot>();

        foreach (var offer in offers)
        {
            var discountPercent = _discountSimulationService.GenerateDiscountPercent();
            if (discountPercent <= 0)
            {
                offer.LastPolledAt = now;
                offer.LastPolledPrice = offer.Price;
                _unitOfWork.GameVendorOffers.Update(offer);
                continue;
            }

            var discountedPrice = Math.Round(offer.Price * (1 - (discountPercent / 100m)), 2, MidpointRounding.AwayFromZero);
            offer.LastPolledAt = now;
            offer.LastPolledPrice = discountedPrice;
            _unitOfWork.GameVendorOffers.Update(offer);

            if (discountPercent < 20)
            {
                continue;
            }

            snapshots.Add(new GameDiscountSnapshot
            {
                Id = Guid.NewGuid(),
                PollingRunId = pollingRunId,
                PolledAt = now,
                GameId = offer.GameId,
                GameName = offer.GameName,
                Vendor = offer.Vendor,
                PurchaseUrl = offer.PurchaseUrl,
                OriginalPrice = offer.Price,
                DiscountedPrice = discountedPrice,
                DiscountPercent = discountPercent,
            });
        }

        var featuredIds = snapshots
            .OrderByDescending(x => x.DiscountPercent)
            .Take(5)
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var snapshot in snapshots)
        {
            snapshot.IsFeatured = featuredIds.Contains(snapshot.Id);
            await _unitOfWork.GameDiscountSnapshots.AddAsync(snapshot);
        }

        await _unitOfWork.SaveChangesAsync();

        if (snapshots.Count > 0)
        {
            List<string> recipients =
            [
                .. (await _unitOfWork.Users.GetAllAsync())
                    .Select(user => $"{user.Name}@gamestore.local"),
            ];

            await _discountNotificationService.NotifyUsersAsync(recipients, snapshots.Select(ToResponse));
        }

        return new DiscountPollingResultResponse
        {
            PollingRunId = pollingRunId,
            PolledAt = now,
            TotalDiscountedGames = snapshots.Count,
            FeaturedGamesCount = snapshots.Count(x => x.IsFeatured),
        };
    }

    public async Task<IReadOnlyCollection<DiscountedGameResponse>> GetLatestFeaturedDiscountsAsync(int take = 5)
    {
        List<GameDiscountSnapshot> snapshots = [.. await _unitOfWork.GameDiscountSnapshots.GetAllAsync()];
        var latestRunId = snapshots
            .OrderByDescending(x => x.PolledAt)
            .Select(x => x.PollingRunId)
            .FirstOrDefault();

        return latestRunId == Guid.Empty
            ? []
            :
            [
                .. snapshots
                    .Where(x => x.PollingRunId == latestRunId && x.IsFeatured)
                    .OrderByDescending(x => x.DiscountPercent)
                    .Take(take)
                    .Select(ToResponse),
            ];
    }

    public async Task<IReadOnlyCollection<GameVendorOfferResponse>> GetOffersByGameKeyAsync(string key)
    {
        var game = await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(key)
            ?? throw new EntityNotFoundException(nameof(Game), key);

        var offers = await _unitOfWork.GameVendorOffers.FindAsync(x => x.GameId == game.Id);
        return
        [
            .. offers.Select(ToOfferResponse),
        ];
    }

    private async Task EnsureOffersSeededAsync()
    {
        var existingOffers = await _unitOfWork.GameVendorOffers.GetCountAsync();
        if (existingOffers > 0)
        {
            return;
        }

        var games = await _unitOfWork.Games.GetAllAsync();
        foreach (var game in games)
        {
            await _unitOfWork.GameVendorOffers.AddAsync(new GameVendorOffer
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                GameName = game.Name,
                Vendor = "GameHub",
                PurchaseUrl = $"https://gamehub.local/{game.Key}",
                Price = Convert.ToDecimal(game.Price),
            });

            await _unitOfWork.GameVendorOffers.AddAsync(new GameVendorOffer
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                GameName = game.Name,
                Vendor = "MetaPlay",
                PurchaseUrl = $"https://metaplay.local/{game.Key}",
                Price = Math.Max(0.99m, Convert.ToDecimal(game.Price) + 3m),
            });
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
}
