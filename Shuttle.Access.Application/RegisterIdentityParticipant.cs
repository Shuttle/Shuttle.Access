using Microsoft.Extensions.Options;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository) : IParticipant<RegisterIdentity>
{
    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);

        var key = Identity.Key(message.Name);

        if (await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
        var aggregate = stream.Get<Identity>();

        var registered = aggregate.Register(message.Name, message.Description, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

        stream.Add(registered);

        if (message.Activated)
        {
            stream.Add(aggregate.Activate(registered.DateRegistered));
        }

        foreach (var roleId in message.RoleIds)
        {
            stream.Add(aggregate.AddRole(roleId));
        }

        await eventStore.SaveAsync(stream, cancellationToken);
    }
}