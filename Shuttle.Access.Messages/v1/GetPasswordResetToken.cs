using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1;

public class GetPasswordResetToken
{
    public string Name { get; set; } = string.Empty;
    public object? PasswordResetToken { get; set; }

    public GetPasswordResetToken WithPasswordResetToken(Guid passwordResetToken)
    {
        PasswordResetToken = Guard.AgainstEmpty(passwordResetToken);
        return this;
    }
}