namespace Shuttle.Access.WebApi.Contracts.v1;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = [];

    public class Specification
    {
        public string NameMatch { get; set; } = string.Empty;
        public bool ShouldIncludePermissions { get; set; }
    }
}