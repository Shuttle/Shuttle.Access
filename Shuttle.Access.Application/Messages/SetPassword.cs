using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetPassword(Guid id, byte[] passwordHash)
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public byte[] PasswordHash { get; } = passwordHash;
}
