using System;
using Shuttle.Access.Events;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IPermissionQueryFactory
    {
        IQuery Search(Query.Permission.Specification specification);
        IQuery Count (Query.Permission.Specification specification);
        IQuery Added(Guid id, Added domainEvent);
        IQuery Activated(Guid id, Activated domainEvent);
        IQuery Deactivated(Guid id, Deactivated domainEvent);
        IQuery Removed(Guid id, Removed domainEvent);
    }
}