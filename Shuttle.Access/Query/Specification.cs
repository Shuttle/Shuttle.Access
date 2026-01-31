namespace Shuttle.Access.SqlServer.Models;

public class Specification<T> where T : class
{
    public int MaximumRows { get; private set; }
    
    private readonly List<Guid> _ids = [];
    private readonly List<Guid> _excludedIds = [];
    public IEnumerable<Guid> Ids => _ids.AsReadOnly();
    public IEnumerable<Guid> ExcludedIds => _ids.AsReadOnly();

    public bool HasIds => _ids.Count > 0;
    public bool HasExcludedIds => _excludedIds.Count > 0;

    public T WithMaximumRows(int maximumRows)
    {
        MaximumRows = maximumRows;

        return GetTypedSpecification();
    }
    
    public T AddId(Guid id)
    {
        if (!_ids.Contains(id))
        {
            _ids.Add(id);
        }
        return GetTypedSpecification();
    }

    public T AddIds(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            AddId(id);
        }
        return GetTypedSpecification();
    }

    public T ExcludeId(Guid id)
    {
        if (!_excludedIds.Contains(id))
        {
            _excludedIds.Add(id);
        }
        return GetTypedSpecification();
    }

    public T ExcludeIds(IEnumerable<Guid> ids)
    {
        foreach (var id in ids)
        {
            ExcludeId(id);
        }
        return GetTypedSpecification();
    }

    private T GetTypedSpecification()
    {
        return this as T ?? throw new ApplicationException($"Could not cast instance of type '{GetType().FullName}' to type '{typeof(T).FullName}'");
    }
}