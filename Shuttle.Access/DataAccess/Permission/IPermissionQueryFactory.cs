using System;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQueryFactory
    {
        IQuery Activated(Guid id, Activated domainEvent);
        IQuery Contains(Query.Permission.Specification specification);
        IQuery Count(Query.Permission.Specification specification);
        IQuery Deactivated(Guid id, Deactivated domainEvent);
        IQuery NameSet(Guid id, NameSet domainEvent);
        IQuery Registered(Guid id, Registered domainEvent);
        IQuery Removed(Guid id, Removed domainEvent);
        IQuery Search(Query.Permission.Specification specification);
    }
}