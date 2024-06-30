using System;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQueryFactory
    {
        IQuery Search(PermissionSpecification specification);
        IQuery Count(PermissionSpecification specification);
        IQuery Registered(Guid id, Registered domainEvent);
        IQuery Activated(Guid id, Activated domainEvent);
        IQuery Deactivated(Guid id, Deactivated domainEvent);
        IQuery Removed(Guid id, Removed domainEvent);
        IQuery Contains(PermissionSpecification specification);
        IQuery NameSet(Guid id, NameSet domainEvent);
    }
}