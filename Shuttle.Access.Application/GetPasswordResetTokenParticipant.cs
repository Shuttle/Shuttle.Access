using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class GetPasswordResetTokenParticipant(IIdentityQuery identityQuery, IEventStore eventStore) : IParticipant<RequestResponseMessage<GetPasswordResetToken, Guid>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task ProcessMessageAsync(RequestResponseMessage<GetPasswordResetToken, Guid> context, CancellationToken cancellationToken = default)
    {
        var identityName = context.Request.Name;
        var query = (await _identityQuery.SearchAsync(new IdentitySpecification().WithName(identityName), cancellationToken)).SingleOrDefault();

        if (query == null)
        {
            context.Failed(string.Format(Access.Resources.UnknownIdentityException, identityName));

            return;
        }

        var stream = await _eventStore.GetAsync(query.Id, cancellationToken: cancellationToken);
        var identity = new Identity();

        stream.Apply(identity);

        if (identity.Activated)
        {
            if (!identity.HasPasswordResetToken)
            {
                stream.Add(identity.RegisterPasswordResetToken());

                await _eventStore.SaveAsync(stream, cancellationToken);
            }

            context.WithResponse(identity.PasswordResetToken!.Value);
        }
        else
        {
            context.Failed(string.Format(Access.Resources.IdentityInactiveException, identityName));
        }
    }
}