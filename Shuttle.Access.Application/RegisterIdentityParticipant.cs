using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IIdentityQuery identityQuery, IRoleQuery roleQuery)
    : IParticipant<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task ProcessMessageAsync(RequestResponseMessage<RegisterIdentity, IdentityRegistered> message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        EventStream stream;
        Identity identity;

        var request = message.Request;
        var key = Identity.Key(request.Name);
        var id = await _idKeyRepository.FindAsync(key, cancellationToken);

        if (id.HasValue)
        {
            identity = new();
            stream = await _eventStore.GetAsync(id.Value, cancellationToken: cancellationToken);

            stream.Apply(identity);

            if (!identity.Removed)
            {
                return;
            }
        }
        else
        {
            id = Guid.NewGuid();
            identity = new();

            await _idKeyRepository.AddAsync(id.Value, key, cancellationToken);

            stream = await _eventStore.GetAsync(id.Value, cancellationToken: cancellationToken);
        }

        var registered = identity.Register(request.Name, request.Description, request.PasswordHash, request.AuditIdentityName, request.GeneratedPassword, request.Activated);

        stream.Add(registered);

        var count = await _identityQuery.CountAsync(new SqlServer.Models.Identity.Specification().WithRoleName("Access Administrator"), cancellationToken);

        if (count == 0)
        {
            var roles = (await _roleQuery.SearchAsync(new SqlServer.Models.Role.Specification().AddName("Access Administrator"), cancellationToken)).ToList();

            if (roles.Count != 1)
            {
                throw new InvalidOperationException(Access.Resources.AdministratorRoleMissingException);
            }

            var role = roles[0];

            if (role.Name.Equals("Access Administrator", StringComparison.InvariantCultureIgnoreCase))
            {
                stream.Add(identity.AddRole(role.Id));
            }
        }

        if (request.Activated)
        {
            stream.Add(identity.Activate(registered.DateRegistered));
        }

        await _eventStore.SaveAsync(stream, cancellationToken);

        message.WithResponse(new()
        {
            Id = id.Value,
            Name = request.Name,
            RegisteredBy = request.AuditIdentityName,
            GeneratedPassword = request.GeneratedPassword,
            System = request.System
        });
    }
}