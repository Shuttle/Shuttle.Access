using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RefreshSession(Guid id)
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
}