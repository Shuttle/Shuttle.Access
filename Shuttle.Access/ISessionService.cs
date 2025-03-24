using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface ISessionCache
{
    Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindByTokenAsync(Guid token, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(string identityName, CancellationToken cancellationToken = default);
    ValueTask<bool> HasPermissionAsync(Guid identityId, string permission, CancellationToken cancellationToken = default);
    Task FlushAsync(CancellationToken cancellationToken = default);
    Task FlushAsync(Guid identityGuid, CancellationToken cancellationToken = default);
}