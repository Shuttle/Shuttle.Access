using Shuttle.Access.Events.Role.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public interface IRoleProjectionQuery
    {
        void Added(PrimitiveEvent primitiveEvent, Added domainEvent);
        void PermissionAdded(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent);
        void PermissionRemoved(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent);
        void Removed(PrimitiveEvent primitiveEvent);
        void NameSet(PrimitiveEvent primitiveEvent, NameSet domainEvent);
    }
}