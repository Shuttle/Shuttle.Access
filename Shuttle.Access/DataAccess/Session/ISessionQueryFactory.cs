using System;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface ISessionQueryFactory
{
    IQuery Save(Session session);
    IQuery AddPermission(Guid token, string permission);
    IQuery Contains(Query.Session.Specification specification);
    IQuery GetPermissions(Guid token);
    IQuery Remove(Guid token);
    IQuery Search(Query.Session.Specification specification);
    IQuery Find(Guid token);
}