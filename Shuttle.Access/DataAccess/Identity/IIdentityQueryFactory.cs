using System;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess
{
    public interface IIdentityQueryFactory
    {
        IQuery Register(Guid id, Registered domainEvent);
        IQuery Count(IdentitySpecification specification);
        IQuery RoleAdded(Guid id, RoleAdded domainEvent);
        IQuery Search(IdentitySpecification specification);
        IQuery Get(Guid id);
        IQuery Roles(IdentitySpecification specification);
        IQuery RoleRemoved(Guid id, RoleRemoved domainEvent);
        IQuery AdministratorCount();
        IQuery RemoveRoles(Guid id);
        IQuery Remove(Guid id);
        IQuery GetId(string identityName);
        IQuery Permissions(Guid id);
        IQuery Activated(Guid id, Activated domainEvent);
        IQuery NameSet(Guid id, NameSet domainEvent);
    }
}