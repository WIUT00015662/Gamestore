using Gamestore.Domain.Entities;

namespace Gamestore.BLL.Filters.Pipes;

public class PublisherFilterPipe(List<Guid>? publisherIds) : IFilterPipe
{
    public IQueryable<T> Execute<T>(IQueryable<T> query) =>
        (publisherIds == null || publisherIds.Count == 0) ? query :
        query.ElementType == typeof(Game) ?
            (IQueryable<T>)((IQueryable<Game>)query)
                .Where(g => publisherIds.Contains(g.PublisherId)) :
            query;
}
