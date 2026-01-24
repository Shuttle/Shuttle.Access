namespace Shuttle.Access.Messages.v1;

public class ResetPassword : AuditMessage
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid PasswordResetToken { get; set; }
}