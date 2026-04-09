using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ResetPasswordParticipant(IHashingService hashingService, IEventStore eventStore, IIdentityQuery identityQuery)
    : IParticipant<ResetPassword>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task HandleAsync(ResetPassword message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var queryIdentity = (await _identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.IdentityName), cancellationToken)).SingleOrDefault();

        if (queryIdentity == null)
        {
            throw new ApplicationException(Access.Resources.InvalidCredentialsException);
        }

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(queryIdentity.Id, cancellationToken);

        stream.Apply(identity);

        if (!identity.HasPasswordResetToken || identity.PasswordResetToken != message.PasswordResetToken)
        {
            throw new ApplicationException(Access.Resources.InvalidCredentialsException);
        }

        stream.Add(identity.SetPassword(_hashingService.Sha256(message.Password)));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.AuditTenantId, message.AuditIdentityName), cancellationToken);
    }
}