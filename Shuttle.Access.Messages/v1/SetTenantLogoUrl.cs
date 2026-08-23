namespace Shuttle.Access.Messages.v1;

public class SetTenantLogoUrl : AuditMessage
{
    public Guid Id { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
}
