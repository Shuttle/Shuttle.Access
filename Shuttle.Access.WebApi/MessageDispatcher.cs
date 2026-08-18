using Microsoft.Extensions.Options;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.WebApi;

public class MessageDispatcher(IOptions<ApiOptions> apiOptions, IMediator mediator, IBus bus)
{
    /// <summary>
    /// Sends the message built by <paramref name="hopperMessage"/> over the Hopper bus when
    /// ApiOptions.UseMessaging is true, else sends the message built by <paramref name="participantMessage"/>
    /// straight to the corresponding Shuttle.Mediator participant.  Only the message actually needed is
    /// constructed, since participant messages validate eagerly (via Guard clauses) in ways the equivalent
    /// Hopper message may not.
    /// </summary>
    public Task DispatchAsync<THopperMessage, TParticipantMessage>(Func<THopperMessage> hopperMessage, Func<TParticipantMessage> participantMessage, CancellationToken cancellationToken = default)
        where THopperMessage : class
        where TParticipantMessage : class
    {
        return apiOptions.Value.UseMessaging
            ? bus.SendAsync(hopperMessage(), cancellationToken)
            : mediator.SendAsync(participantMessage(), cancellationToken);
    }
}
