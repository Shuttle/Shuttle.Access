using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ResetPasswordParticipant(IHashingService hashingService, IEventStore eventStore, IIdentityQuery identityQuery)
    : IParticipant<RequestMessage<ResetPassword>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task ProcessMessageAsync(RequestMessage<ResetPassword> message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var queryIdentity = (await _identityQuery.SearchAsync(new SqlServer.Models.Identity.Specification().WithName(message.Request.Name), cancellationToken)).SingleOrDefault();

        if (queryIdentity == null)
        {
            message.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(queryIdentity.Id, cancellationToken);

        stream.Apply(identity);

        if (!identity.HasPasswordResetToken || identity.PasswordResetToken != message.Request.PasswordResetToken)
        {
            message.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        stream.Add(identity.SetPassword(_hashingService.Sha256(message.Request.Password)));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.Request), cancellationToken);
    }
}