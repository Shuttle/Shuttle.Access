using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class ResetPassword(string identityName, string password, Guid passwordResetToken, Guid auditTenantId, string auditIdentityName)
{
    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public string Password { get; } = Guard.AgainstEmpty(password);
    public Guid PasswordResetToken { get; } = Guard.AgainstEmpty(passwordResetToken);
}