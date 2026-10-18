using Microsoft.Extensions.DependencyInjection;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;

namespace Shuttle.Access.AspNetCore;

public static class HopperBuilderExtensions
{
    /// <summary>
    ///     Subscribes to the session-change messages Shuttle.Access publishes (<see cref="SessionDeleted" />,
    ///     <see cref="AllSessionsDeleted" />, <see cref="SessionRefreshed" />) and registers the handler that uses
    ///     them to evict stale entries from <see cref="DelegatedSessionResolver" />'s cache.
    /// </summary>
    /// <remarks>
    ///     Call this once, alongside the application's other Hopper configuration, in every application that uses
    ///     <see cref="DelegatedSessionResolver" /> — i.e. every application other than the Shuttle.Access web API
    ///     itself. Without it, the cache still works, but falls back to its own short expiry to notice a session
    ///     change rather than being told about it immediately.
    /// </remarks>
    public static HopperBuilder AddAccessSessionCacheInvalidation(this HopperBuilder hopperBuilder)
    {
        Guard.AgainstNull(hopperBuilder);

        return hopperBuilder
            .AddSubscription<SessionDeleted>()
            .AddSubscription<AllSessionsDeleted>()
            .AddSubscription<SessionRefreshed>()
            .AddMessageHandler<SessionCacheInvalidationHandler>(ServiceLifetime.Singleton);
    }
}
