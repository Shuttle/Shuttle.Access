using System;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class GetPasswordResetTokenParticipant : IAsyncParticipant<RequestResponseMessage<GetPasswordResetToken, Guid>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdentityQuery _identityQuery;

    public GetPasswordResetTokenParticipant(IIdentityQuery identityQuery, IEventStore eventStore)
    {
        _identityQuery = identityQuery;
        _eventStore = eventStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<GetPasswordResetToken, Guid>> context)
    {
        var identityName = context.Message.Request.Name;
        var query = (await _identityQuery.SearchAsync(new DataAccess.Query.Identity.Specification().WithName(identityName))).SingleOrDefault();

        if (query == null)
        {
            context.Message.Failed(string.Format(Access.Resources.UnknownIdentityException, identityName));

            return;
        }

        var stream = await _eventStore.GetAsync(query.Id);
        var identity = new Identity();

        stream.Apply(identity);

        if (identity.Activated)
        {
            if (!identity.HasPasswordResetToken)
            {
                stream.AddEvent(identity.RegisterPasswordResetToken());

                await _eventStore.SaveAsync(stream);
            }

            context.Message.WithResponse(identity.PasswordResetToken.Value);
        }
        else
        {
            context.Message.Failed(string.Format(Access.Resources.IdentityInactiveException, identityName));
        }
    }
}