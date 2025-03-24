using System;

namespace Shuttle.Access.Application;

public class RefreshSession
{
    public Guid IdentityId { get; } 

    public RefreshSession(Guid identityId)
    {
        IdentityId = identityId;
    }
}