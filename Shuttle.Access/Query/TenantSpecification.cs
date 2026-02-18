using Shuttle.Access.SqlServer.Models;

namespace Shuttle.Access.Query;

public class TenantSpecification : Specification<TenantSpecification>
{
    private readonly List<string> _names = [];
    public string NameMatch { get; private set; } = string.Empty;
    public IEnumerable<string> Names => _names.AsReadOnly();

    public TenantSpecification AddName(string name)
    {
        if (!_names.Contains(name))
        {
            _names.Add(name);
        }

        return this;
    }

    public TenantSpecification WithNameMatch(string nameMatch)
    {
        NameMatch = nameMatch;

        return this;
    }
}