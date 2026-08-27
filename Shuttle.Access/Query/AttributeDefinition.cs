using Shuttle.Contract;

namespace Shuttle.Access.Query;

public class AttributeDefinition
{
    public AttributeCardinality Cardinality { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public AttributeType Type { get; set; }

    public class Specification : Specification<Specification>
    {
        private readonly List<string> _names = [];
        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public Guid? TenantId { get; private set; }

        public Specification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }

        public Specification WithTenantId(Guid tenantId)
        {
            TenantId = Guard.AgainstEmpty(tenantId);

            return this;
        }
    }
}
