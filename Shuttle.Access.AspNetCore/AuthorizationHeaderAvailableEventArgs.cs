using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AuthorizationHeaderAvailableEventArgs(string value)
{
    public string Value { get; } = Guard.AgainstEmpty(value);
}