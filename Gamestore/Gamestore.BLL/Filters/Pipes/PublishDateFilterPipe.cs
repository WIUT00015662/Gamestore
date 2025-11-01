using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class PublishDateFilterPipe(string? publishDateFilter) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query)
    {
        if (string.IsNullOrEmpty(publishDateFilter))
        {
            return query;
        }

        if (query.ElementType == typeof(Game))
        {
            var now = DateTime.UtcNow;
            var gameQuery = (IQueryable<Game>)query;

            var filterDate = publishDateFilter.ToLowerInvariant() switch
            {
                "last week" => now.AddDays(-7),
                "last month" => now.AddMonths(-1),
                "last year" => now.AddYears(-1),
                "2 years" => now.AddYears(-2),
                "3 years" => now.AddYears(-3),
                _ => now,
            };

            gameQuery = gameQuery.Where(g => g.PublishDate >= filterDate);
            return (IQueryable<T>)gameQuery;
        }

        return query;
    }
}
