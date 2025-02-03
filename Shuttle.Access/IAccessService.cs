using System;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface IAccessService
{
    ValueTask<bool> ContainsAsync(Guid token);
    void Flush(Guid token);
    ValueTask<bool> HasPermissionAsync(Guid token, string permission);
    void Remove(Guid token);
}