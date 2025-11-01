namespace Gamestore.BLL.Filters;

public class FilterPipeline : IFilterPipeline
{
    private readonly List<IFilterPipe> _pipes = [];

    public IFilterPipeline AddPipe(IFilterPipe pipe)
    {
        _pipes.Add(pipe);
        return this;
    }

    public IQueryable<T> Execute<T>(IQueryable<T> query)
    {
        foreach (var pipe in _pipes)
        {
            query = pipe.Execute(query);
        }

        return query;
    }
}
