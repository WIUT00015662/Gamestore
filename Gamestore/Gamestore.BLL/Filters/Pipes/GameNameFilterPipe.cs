using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.BLL.Filters.Pipes;

public class GameNameFilterPipe(string? gameName) : IFilterPipe
{
    private const int MinNameLength = 3;

    public IQueryable<T> Execute<T>(IQueryable<T> query)
    {
        if (string.IsNullOrEmpty(gameName) || gameName.Length < MinNameLength)
        {
            return query;
        }

        if (query.ElementType != typeof(Game))
        {
            return query;
        }

        var gameQuery = (IQueryable<Game>)query;

        var filteredGameQuery = gameQuery.Where(g =>
            EF.Functions.Like(g.Name, $"%{gameName}%"));

        return (IQueryable<T>)filteredGameQuery;
    }
}
