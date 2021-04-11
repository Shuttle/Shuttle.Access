using System;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IIdentityQueryFactory
    {
        IQuery Register(Guid id, Registered domainEvent);
        IQuery Count(Query.Identity.Specification specification);
        IQuery RoleAdded(Guid id, RoleAdded domainEvent);
        IQuery Search(Query.Identity.Specification specification);
        IQuery Get(Guid id);
        IQuery Roles(Query.Identity.Specification specification);
        IQuery Roles(Guid userId);
        IQuery RoleRemoved(Guid id, RoleRemoved domainEvent);
        IQuery AdministratorCount();
        IQuery RemoveRoles(Guid id, Removed domainEvent);
        IQuery Remove(Guid id, Removed domainEvent);
        IQuery GetId(string identityName);
        IQuery Permissions(Guid id);
        IQuery Activated(Guid id, Activated domainEvent);
    }
}