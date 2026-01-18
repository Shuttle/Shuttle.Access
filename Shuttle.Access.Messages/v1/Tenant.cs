namespace Shuttle.Access.Messages.v1;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;

    public class Specification
    {
        public string Name { get; set; } = string.Empty;
        public string NameMatch { get; set; } = string.Empty;
    }
}