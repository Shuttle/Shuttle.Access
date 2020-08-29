using System;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface ISystemUserQueryFactory
    {
        IQuery Register(Guid id, Registered domainEvent);
        IQuery Count(Query.User.Specification specification);
        IQuery RoleAdded(Guid id, RoleAdded domainEvent);
        IQuery Search(Query.User.Specification specification);
        IQuery Get(Guid id);
        IQuery Roles(Query.User.Specification specification);
        IQuery Roles(Guid userId);
        IQuery RoleRemoved(Guid id, RoleRemoved domainEvent);
        IQuery AdministratorCount();
        IQuery RemoveRoles(Guid id, Removed domainEvent);
        IQuery Remove(Guid id, Removed domainEvent);
    }
}