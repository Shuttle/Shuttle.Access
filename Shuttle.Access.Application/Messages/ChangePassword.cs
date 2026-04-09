using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class ChangePassword : IAuditInformation
{
    public Guid AuditTenantId { get; }
    public string AuditIdentityName { get; }

    private ChangePassword(string newPassword, Guid auditTenantId, string auditIdentityName)
    {
        AuditTenantId = Guard.AgainstEmpty(auditTenantId);
        AuditIdentityName = Guard.AgainstEmpty(auditIdentityName);
        NewPassword = Guard.AgainstEmpty(newPassword);
    }

    public Guid? Id { get; set; }
    public string NewPassword { get; }
    public Guid? Token { get; set; }

    public static ChangePassword UseId(Guid id, string newPassword, Guid auditTenantId, string auditIdentityName)
    {
        return new(newPassword, auditTenantId, auditIdentityName)
        {
            Id = id
        };
    }

    public static ChangePassword UseToken(Guid token, string newPassword, Guid auditTenantId, string auditIdentityName)
    {
        return new(newPassword, auditTenantId, auditIdentityName)
        {
            Token = token
        };
    }
}