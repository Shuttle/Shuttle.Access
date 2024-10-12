using System;

namespace Shuttle.Access
{
    public interface IAccessService
    {
        bool Contains(Guid token);
        bool HasPermission(Guid token, string permission);
        void Remove(Guid token);
        void Flush(Guid token);
    }
}