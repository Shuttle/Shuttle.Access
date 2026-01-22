namespace Shuttle.Access.Messages.v1;

public class RegisterTenant : AuditMessage
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
}