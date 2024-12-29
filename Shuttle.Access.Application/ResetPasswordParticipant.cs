using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ResetPasswordParticipant : IParticipant<RequestMessage<ResetPassword>>
{
    private readonly IEventStore _eventStore;
    private readonly IHashingService _hashingService;
    private readonly IIdentityQuery _identityQuery;

    public ResetPasswordParticipant(IHashingService hashingService, IEventStore eventStore, IIdentityQuery identityQuery)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _hashingService = Guard.AgainstNull(hashingService);
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestMessage<ResetPassword>> context)
    {
        Guard.AgainstNull(context);

        var queryIdentity = (await _identityQuery.SearchAsync(new DataAccess.Query.Identity.Specification().WithName(context.Message.Request.Name))).SingleOrDefault();

        if (queryIdentity == null)
        {
            context.Message.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(queryIdentity.Id);

        stream.Apply(identity);

        if (!identity.HasPasswordResetToken || identity.PasswordResetToken != context.Message.Request.PasswordResetToken)
        {
            context.Message.Failed(Access.Resources.InvalidCredentialsException);

            return;
        }

        stream.Add(identity.SetPassword(_hashingService.Sha256(context.Message.Request.Password)));

        await _eventStore.SaveAsync(stream);
    }
}