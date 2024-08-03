using System;
using Shuttle.Access.DataAccess.Query;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQueryFactory
    {
        IQuery Get(Guid token);
        IQuery Get(string identityName);
        IQuery GetPermissions(Guid token);
        IQuery Remove(string identityName);
        IQuery Add(Access.Session session);
        IQuery AddPermission(Guid token, string permission);
        IQuery Remove(Guid token);
        IQuery Contains(Guid token);
        IQuery Contains(Guid token, string permission);
        IQuery Renew(Access.Session session);
        IQuery Search(Query.Session.Specification specification);
    }
}