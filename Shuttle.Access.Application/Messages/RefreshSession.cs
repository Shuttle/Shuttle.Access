namespace Shuttle.Access.Application;

public class RefreshSession
{
    public RefreshSession(Guid identityId)
    {
        IdentityId = identityId;
    }

    public Guid IdentityId { get; }
}