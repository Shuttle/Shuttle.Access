using Shuttle.Access.Events.Permission.v1;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public interface IPermissionProjectionQuery
    {
        void Registered(PrimitiveEvent primitiveEvent, Registered domainEvent);
        void Activated(PrimitiveEvent primitiveEvent, Activated domainEvent);
        void Deactivated(PrimitiveEvent primitiveEvent, Deactivated domainEvent);
        void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent);
    }
}