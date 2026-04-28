using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class SortingPipe(string? sortBy) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query;
        }

        if (query.ElementType == typeof(Game))
        {
            var gameQuery = (IQueryable<Game>)query;

            gameQuery = sortBy.ToLowerInvariant() switch
            {
                "most popular" => gameQuery.OrderByDescending(g => g.ViewCount),
                "most commented" => gameQuery.OrderByDescending(g => g.Comments.Count),
                "price asc" => gameQuery.OrderBy(g => g.VendorOffers.Min(o => o.CurrentPrice)),
                "price desc" => gameQuery.OrderByDescending(g => g.VendorOffers.Min(o => o.CurrentPrice)),
                "new" => gameQuery.OrderByDescending(g => g.PublishDate),
                _ => gameQuery,
            };

            return (IQueryable<T>)gameQuery;
        }

        return query;
    }
}
