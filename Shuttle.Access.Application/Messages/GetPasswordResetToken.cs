using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class GetPasswordResetToken(Guid identityId)
{
    public Guid IdentityId { get; } = Guard.AgainstEmpty(identityId);
    public object? PasswordResetToken { get; private set; }

    public GetPasswordResetToken WithPasswordResetToken(Guid passwordResetToken)
    {
        PasswordResetToken = Guard.AgainstEmpty(passwordResetToken);
        return this;
    }
}