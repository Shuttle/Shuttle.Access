using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class PromoteIdentityAlternateNameParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<PromoteIdentityAlternateName>
{
    public async Task HandleAsync(PromoteIdentityAlternateName message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.Id, cancellationToken);

        stream.Apply(identity);

        if (!identity.AlternateNames.Contains(message.Name))
        {
            throw new InvalidOperationException(string.Format(Shuttle.Access.Resources.IdentityAlternateNameNotFoundException, message.Name, identity.Name));
        }

        var previousPrimaryName = identity.Name;

        // The alternate being promoted already has its own key in the id-key store (registered when it
        // was added as an alternate), so promoting it to primary never needs a rekey.
        stream.Add(identity.RemoveAlternateName(message.Name));
        stream.Add(identity.SetName(message.Name));

        switch (message.Mode)
        {
            case IdentityAlternateNamePromotionMode.Swap:
            {
                stream.Add(identity.AddAlternateName(previousPrimaryName));
                break;
            }
            case IdentityAlternateNamePromotionMode.Replace:
            {
                // The previous primary name is discarded entirely, so its key can no longer resolve to this identity.
                await idKeyRepository.RemoveAsync(Identity.Key(previousPrimaryName), cancellationToken);
                break;
            }
            default:
            {
                throw new InvalidOperationException($"Unknown '{nameof(IdentityAlternateNamePromotionMode)}' value '{message.Mode}'.");
            }
        }

        await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
    }
}
