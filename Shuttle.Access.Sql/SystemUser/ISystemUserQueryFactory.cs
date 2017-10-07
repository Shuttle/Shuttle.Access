using System;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql.SystemUser
{
    public interface ISystemUserQueryFactory
    {
        IQuery Register(Guid id, Registered domainEvent);
        IQuery Count();
        IQuery RoleAdded(Guid id, RoleAdded domainEvent);
        IQuery Search();
        IQuery Get(Guid id);
        IQuery Roles(Guid id);
        IQuery RoleRemoved(Guid id, RoleRemoved domainEvent);
        IQuery AdministratorCount();
        IQuery RemoveRoles(Guid id, Removed domainEvent);
        IQuery Remove(Guid id, Removed domainEvent);
    }
}