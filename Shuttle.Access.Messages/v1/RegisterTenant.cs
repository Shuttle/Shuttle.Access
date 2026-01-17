namespace Shuttle.Access.Messages.v1;

public class RegisterTenant
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string AuditIdentityName { get; set; } = string.Empty;
}