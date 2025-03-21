﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Access;

public abstract class SessionCache
{
    private readonly List<SessionEntry> _sessionEntries = new();

    private Messages.v1.Session? ActiveSessionOnly(Messages.v1.Session? session)
    {
        if (session != null &&
            session.ExpiryDate > DateTimeOffset.UtcNow)
        {
            Flush(session.IdentityId);

            return null;
        }

        return session;
    }

    protected Messages.v1.Session? FindByToken(Guid token)
    {
        return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Token.HasValue && item.Token.Equals(token))?.Session);
    }

    protected Messages.v1.Session? Find(Guid identityId)
    {
        return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Session.IdentityId.Equals(identityId))?.Session);
    }

    protected Messages.v1.Session? Find(string identityName)
    {
        return ActiveSessionOnly(_sessionEntries.FirstOrDefault(item => item.Session.IdentityName.Equals(identityName, StringComparison.InvariantCultureIgnoreCase))?.Session);
    }

    protected Messages.v1.Session Add(Guid? token, Messages.v1.Session session)
    {
        _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(session.IdentityId));
        _sessionEntries.Add(new(token, session));

        return session;
    }

    public void Flush()
    {
        _sessionEntries.Clear();
    }

    protected void Flush(Guid identityId)
    {
        _sessionEntries.RemoveAll(item => item.Session.IdentityId.Equals(identityId));
    }

    protected bool HasPermission(Guid identityId, string permission, string administratorPermissionName)
    {
        var sessionEntry = _sessionEntries.FirstOrDefault(item => item.Session.IdentityId.Equals(identityId));

        if (sessionEntry == null)
        {
            return false;
        }

        return sessionEntry.Session.Permissions.Contains(permission) ||  (!string.IsNullOrWhiteSpace(administratorPermissionName) && sessionEntry.Session.Permissions.Contains("*"));
    }

    private class SessionEntry
    {
        public SessionEntry(Guid? token, Messages.v1.Session session)
        {
            Session = session;
            Token = token;
        }

        public Messages.v1.Session Session { get; }

        public Guid? Token { get; set; }
    }
}