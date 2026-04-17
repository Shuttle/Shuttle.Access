using Shuttle.Contract;

namespace Shuttle.Access;

public class AuthenticationEventArgs(string identityName)
{
    public string IdentityName { get; private set; } = Guard.AgainstEmpty(identityName);
}