using Gamestore.BLL.DTOs.Game;
using Gamestore.BLL.DTOs.Genre;
using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.DTOs.Platform;
using Gamestore.BLL.DTOs.Publisher;

namespace Gamestore.BLL.Mapping;

internal static class DomainMappingExtensions
{
    public static GameResponse ToResponse(this Domain.Entities.Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            Key = game.Key,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discount = game.Discount,
        };
    }

    public static IEnumerable<GameResponse> ToResponse(this IEnumerable<Domain.Entities.Game> games)
    {
        return games.Select(g => g.ToResponse());
    }

    public static GenreResponse ToResponse(this Domain.Entities.Genre genre)
    {
        return new GenreResponse
        {
            Id = genre.Id,
            Name = genre.Name,
            ParentGenreId = genre.ParentGenreId,
        };
    }

    public static IEnumerable<GenreResponse> ToResponse(this IEnumerable<Domain.Entities.Genre> genres)
    {
        return genres.Select(g => g.ToResponse());
    }

    public static PlatformResponse ToResponse(this Domain.Entities.Platform platform)
    {
        return new PlatformResponse
        {
            Id = platform.Id,
            Type = platform.Type,
        };
    }

    public static IEnumerable<PlatformResponse> ToResponse(this IEnumerable<Domain.Entities.Platform> platforms)
    {
        return platforms.Select(p => p.ToResponse());
    }

    public static PublisherResponse ToResponse(this Domain.Entities.Publisher publisher)
    {
        return new PublisherResponse
        {
            Id = publisher.Id,
            CompanyName = publisher.CompanyName,
            HomePage = publisher.HomePage,
            Description = publisher.Description,
        };
    }

    public static IEnumerable<PublisherResponse> ToResponse(this IEnumerable<Domain.Entities.Publisher> publishers)
    {
        return publishers.Select(x => x.ToResponse());
    }

    public static OrderResponse ToOrderResponse(this Domain.Entities.Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Date = order.Date,
        };
    }

    public static IEnumerable<OrderResponse> ToOrderResponse(this IEnumerable<Domain.Entities.Order> orders)
    {
        return orders.Select(o => o.ToOrderResponse());
    }

    public static OrderGameResponse ToOrderGameResponse(this Domain.Entities.OrderGame orderGame)
    {
        return new OrderGameResponse
        {
            ProductId = orderGame.ProductId,
            Price = orderGame.Price,
            Quantity = orderGame.Quantity,
            Discount = orderGame.Discount,
        };
    }

    public static IEnumerable<OrderGameResponse> ToOrderGameResponse(this IEnumerable<Domain.Entities.OrderGame> orderGames)
    {
        return orderGames.Select(og => og.ToOrderGameResponse());
    }
}
