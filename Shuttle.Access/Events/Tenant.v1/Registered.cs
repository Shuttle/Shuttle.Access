namespace Shuttle.Access.Events.Tenant.v1;

public class Registered
{
    public string Name { get; set; } = string.Empty;
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public int Status { get; set; }
}