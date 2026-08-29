namespace Shuttle.Access.Messages.v1;

public class RegisterTenant : AuditMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public string AdministratorIdentityName { get; set; } = string.Empty;
    public Guid AccessAdministratorRoleId { get; set; }
}