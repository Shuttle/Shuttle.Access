using Microsoft.Extensions.Options;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterIdentityParticipant(IOptions<AccessOptions> accessOptions, IEventStore eventStore, IIdKeyRepository idKeyRepository, IIdentityQuery identityQuery, IRoleQuery roleQuery)
    : IParticipant<RegisterIdentity>
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        EventStream stream;
        Identity identity;

        var key = Identity.Key(message.Name);
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
            id =  message.Id ?? Guid.NewGuid();
            identity = new();

            await _idKeyRepository.AddAsync(id.Value, key, cancellationToken);

            stream = await _eventStore.GetAsync(id.Value, cancellationToken: cancellationToken);
        }

        var registered = identity.Register(message.Name, message.Description, message.PasswordHash, message.AuditIdentityName, message.GeneratedPassword, message.Activated);

        stream.Add(registered);

        var count = await _identityQuery.CountAsync(new IdentitySpecification().WithRoleName("Access Administrator"), cancellationToken);

        if (count == 0)
        {
            var roles = (await _roleQuery.SearchAsync(new RoleSpecification()
                .WithTenantId(_accessOptions.SystemTenantId)
                .AddName("Access Administrator"), cancellationToken)).ToList();

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

        if (message.Activated)
        {
            stream.Add(identity.Activate(registered.DateRegistered));
        }

        await _eventStore.SaveAsync(stream, cancellationToken);
    }
}