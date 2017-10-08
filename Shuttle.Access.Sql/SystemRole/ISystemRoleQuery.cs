using System;
using System.Collections.Generic;
using System.Data;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public interface ISystemRoleQuery
    {
        IEnumerable<string> Permissions(string roleName);
        IEnumerable<DataRow> Search();
        Query.Role Get(Guid id);
        IEnumerable<string> Permissions(Guid id);

        void Added(PrimitiveEvent primitiveEvent, Added domainEvent);
        void PermissionAdded(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent);
        void PermissionRemoved(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent);
        void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent);
    }
}