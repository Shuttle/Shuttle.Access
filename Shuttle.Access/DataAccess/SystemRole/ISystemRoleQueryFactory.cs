using System;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemRoleQueryFactory
    {
        IQuery Permissions(string roleName);
        IQuery Permissions(Guid roleId);
        IQuery Search(Query.Role.Specification specification);
        IQuery Added(Guid id, Added domainEvent);
        IQuery Get(Guid id);
        IQuery PermissionAdded(Guid id, PermissionAdded domainEvent);
        IQuery PermissionRemoved(Guid id, PermissionRemoved domainEvent);
        IQuery Removed(Guid id, Removed domainEvent);
    }
}