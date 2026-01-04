namespace Shuttle.Access.Application;

public class RefreshSession(Guid identityId)
{
    public Guid IdentityId { get; } = identityId;
}