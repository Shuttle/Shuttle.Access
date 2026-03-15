using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class GetPasswordResetTokenParticipant(IIdentityQuery identityQuery, IEventStore eventStore) : IParticipant<GetPasswordResetToken>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task HandleAsync(GetPasswordResetToken message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var model = (await _identityQuery.SearchAsync(new Query.Identity.Specification().AddId(message.IdentityId), cancellationToken)).SingleOrDefault();

        if (model == null)
        {
            throw new ApplicationException(string.Format(Access.Resources.UnknownIdentityIdException, message.IdentityId));
        }

        var stream = await _eventStore.GetAsync(model.Id, cancellationToken: cancellationToken);
        var identity = new Identity();

        stream.Apply(identity);

        if (identity.Activated)
        {
            if (!identity.HasPasswordResetToken)
            {
                stream.Add(identity.RegisterPasswordResetToken());

                await _eventStore.SaveAsync(stream, cancellationToken);
            }

            message.WithPasswordResetToken(identity.PasswordResetToken!.Value);
        }
        else
        {
            throw new ApplicationException(string.Format(Access.Resources.IdentityInactiveException, model.Name));
        }
    }
}