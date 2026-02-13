using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
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

    public async Task ProcessMessageAsync(RequestMessage<ResetPassword> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var queryIdentity = (await _identityQuery.SearchAsync(new IdentitySpecification().WithName(context.Request.Name), cancellationToken)).SingleOrDefault();

        if (queryIdentity == null)
        {
            context.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(queryIdentity.Id, cancellationToken);

        stream.Apply(identity);

        if (!identity.HasPasswordResetToken || identity.PasswordResetToken != context.Request.PasswordResetToken)
        {
            context.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        stream.Add(identity.SetPassword(_hashingService.Sha256(context.Request.Password)));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(context.Request), cancellationToken);
    }
}