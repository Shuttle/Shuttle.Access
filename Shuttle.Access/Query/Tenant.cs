namespace Shuttle.Access.Query;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;

    public class Specification : Specification<Specification>
    {
        private readonly List<string> _names = [];
        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public bool ShouldIncludeActiveOnly { get; private set; }

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

        public Specification IncludeActiveOnly()
        {
            ShouldIncludeActiveOnly = true;
            return this;
        }
    }
}