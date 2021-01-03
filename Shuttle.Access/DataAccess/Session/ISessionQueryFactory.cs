using System;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQueryFactory
    {
        IQuery Get(Guid token);
        IQuery GetPermissions(Guid token);
        IQuery Remove(string identityName);
        IQuery Add(Session session);
        IQuery AddPermission(Guid token, string permission);
        IQuery Remove(Guid token);
        IQuery Contains(Guid token);
        IQuery Contains(Guid token, string permission);
        IQuery Renew(Session session);
    }
}