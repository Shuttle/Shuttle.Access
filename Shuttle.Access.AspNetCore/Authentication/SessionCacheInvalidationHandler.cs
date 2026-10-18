using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Keeps <see cref="IDelegatedSessionCache" /> correct by evicting entries as soon as Shuttle.Access publishes
///     that a session has changed, instead of relying solely on the cache entry's own expiry.
/// </summary>
/// <remarks>
///     Registered by <see cref="HopperBuilderExtensions.AddAccessSessionCacheInvalidation" />; it is not picked up
///     automatically, since not every application that configures Hopper also uses
///     <see cref="DelegatedSessionResolver" />.
/// </remarks>
public class SessionCacheInvalidationHandler(IDelegatedSessionCache delegatedSessionCache) :
    IMessageHandler<SessionDeleted>,
    IMessageHandler<SessionRefreshed>,
    IMessageHandler<AllSessionsDeleted>
{
    private readonly IDelegatedSessionCache _delegatedSessionCache = Guard.AgainstNull(delegatedSessionCache);

    public Task HandleAsync(SessionDeleted message, CancellationToken cancellationToken = default)
    {
        _delegatedSessionCache.Flush(Guard.AgainstNull(message).IdentityId);

        return Task.CompletedTask;
    }

    public Task HandleAsync(SessionRefreshed message, CancellationToken cancellationToken = default)
    {
        _delegatedSessionCache.Flush(Guard.AgainstNull(message).IdentityId);

        return Task.CompletedTask;
    }

    public Task HandleAsync(AllSessionsDeleted message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        _delegatedSessionCache.Flush();

        return Task.CompletedTask;
    }
}
