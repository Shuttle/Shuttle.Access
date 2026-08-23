namespace Shuttle.Access.Messages.v1;

public class SetTenantLogoSvg : AuditMessage
{
    public Guid Id { get; set; }
    public string LogoSvg { get; set; } = string.Empty;
}
