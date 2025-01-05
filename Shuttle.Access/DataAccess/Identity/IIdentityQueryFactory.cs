using System;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public interface IIdentityQueryFactory
{
    IQuery Activated(Guid id, Activated domainEvent);
    IQuery AdministratorCount();
    IQuery Count(Identity.Specification specification);
    IQuery Get(Guid id);
    IQuery GetId(string identityName);
    IQuery NameSet(Guid id, NameSet domainEvent);
    IQuery Permissions(Guid id);
    IQuery Register(Guid id, Registered domainEvent);
    IQuery Remove(Guid id);
    IQuery RemoveRoles(Guid id);
    IQuery RoleAdded(Guid id, RoleAdded domainEvent);
    IQuery RoleRemoved(Guid id, RoleRemoved domainEvent);
    IQuery Roles(Identity.Specification specification);
    IQuery Search(Identity.Specification specification);
}