using Microsoft.Extensions.Options;
using Shuttle.Hopper;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.WebApi;

public class MessageDispatcher(IOptions<RecallOptions> recallOptions, IMediator mediator, IBus bus)
{
    /// <summary>
    /// Sends the message built by <paramref name="hopperMessage"/> over the Hopper bus when
    /// RecallOptions.EventProcessing.ImmediateConsistency.Enabled is false, else sends the message built by
    /// <paramref name="participantMessage"/> straight to the corresponding Shuttle.Mediator participant so that
    /// the event is processed synchronously, in-process, and immediate consistency is actually achieved.  Only
    /// the message actually needed is constructed, since participant messages validate eagerly (via Guard
    /// clauses) in ways the equivalent Hopper message may not.
    /// </summary>
    public Task DispatchAsync<THopperMessage, TParticipantMessage>(Func<THopperMessage> hopperMessage, Func<TParticipantMessage> participantMessage, CancellationToken cancellationToken = default)
        where THopperMessage : class
        where TParticipantMessage : class
    {
        return recallOptions.Value.EventProcessing.ImmediateConsistency.Enabled
            ? mediator.SendAsync(participantMessage(), cancellationToken)
            : bus.SendAsync(hopperMessage(), cancellationToken);
    }
}
