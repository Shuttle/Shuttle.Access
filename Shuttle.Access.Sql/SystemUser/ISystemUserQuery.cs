using System;
using System.Collections.Generic;
using System.Data;

namespace Shuttle.Access.Sql
{
    public interface ISystemUserQuery
    {
        int Count();
        IEnumerable<DataRow> Search();
        Query.User Get(Guid id);
        IEnumerable<string> Roles(Guid id);

        void Register(PrimitiveEvent primitiveEvent, Registered domainEvent);
        void RoleAdded(PrimitiveEvent primitiveEvent, RoleAdded domainEvent);
        void RoleRemoved(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent);
        int AdministratorCount();
        void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent);
    }
}