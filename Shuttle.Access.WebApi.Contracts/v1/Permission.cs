namespace Shuttle.Access.WebApi.Contracts.v1;

public class Permission
{
    public string Description { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public class Specification
    {
        public List<Guid> Ids { get; set; } = [];
        public string NameMatch { get; set; } = string.Empty;
    }
}