using System;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface ISessionQueryFactory
{
    IQuery Save(Access.Session session);
    IQuery Contains(Session.Specification specification);
    IQuery GetPermissions(Guid token);
    IQuery Remove(Guid token);
    IQuery Search(Session.Specification specification);
    IQuery Find(Guid token);
    IQuery Count(Session.Specification specification);
    IQuery RemoveAll();
}