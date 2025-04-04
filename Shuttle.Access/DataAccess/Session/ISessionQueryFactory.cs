using System;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface ISessionQueryFactory
{
    IQuery Save(Access.Session session);
    IQuery Contains(Session.Specification specification);
    IQuery GetPermissions(Guid identityId);
    IQuery Remove(Guid identityId);
    IQuery Search(Session.Specification specification);
    IQuery Count(Session.Specification specification);
    IQuery RemoveAll();
}