using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class SessionUnavailableEventArgs(string identifierType, string identifier)
{
    public string IdentifierType { get; } = Guard.AgainstEmpty(identifierType);
    public string Identifier { get; } = Guard.AgainstEmpty(identifier);
}