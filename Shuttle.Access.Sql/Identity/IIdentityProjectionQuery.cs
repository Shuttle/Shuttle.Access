using Shuttle.Access.Events.Identity.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public interface IIdentityProjectionQuery
    {
        void Register(PrimitiveEvent primitiveEvent, Registered domainEvent);
        void RoleAdded(PrimitiveEvent primitiveEvent, RoleAdded domainEvent);
        void RoleRemoved(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent);
        void Removed(PrimitiveEvent primitiveEvent);
        void Activated(PrimitiveEvent primitiveEvent, Activated domainEvent);
        void NameSet(PrimitiveEvent primitiveEvent, NameSet domainEvent);
    }
}