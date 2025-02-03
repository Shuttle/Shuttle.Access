using System;

namespace Shuttle.Access.Application;

public class RefreshSession
{
    public string IdentityName { get; } = string.Empty;
    public Guid? Token { get; }

    public RefreshSession(Guid token)
    {
        Token = token;
    }

    public RefreshSession(string identityName)
    {
        IdentityName = identityName;
    }
}