using System;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IRoleQueryFactory
    {
        IQuery Search(RoleSpecification specification);
        IQuery Registered(Guid id, Registered domainEvent);
        IQuery Get(Guid id);
        IQuery PermissionAdded(Guid id, PermissionAdded domainEvent);
        IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent);
        IQuery Removed(Guid id);
        IQuery Count(RoleSpecification specification);
        IQuery Permissions(RoleSpecification specification);
        IQuery NameSet(Guid id, NameSet domainEvent);
    }
}