using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class PriceFilterPipe(double? minPrice, double? maxPrice) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query)
    {
        if (minPrice == null && maxPrice == null)
        {
            return query;
        }

        if (query.ElementType == typeof(Game))
        {
            var gameQuery = (IQueryable<Game>)query;

            if (minPrice.HasValue)
            {
                gameQuery = gameQuery.Where(g => g.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                gameQuery = gameQuery.Where(g => g.Price <= maxPrice.Value);
            }

            return (IQueryable<T>)gameQuery;
        }

        return query;
    }
}
