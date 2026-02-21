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
    : IParticipant<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IIdKeyRepository _idKeyRepository = Guard.AgainstNull(idKeyRepository);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task HandleAsync(RequestResponseMessage<RegisterIdentity, IdentityRegistered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        EventStream stream;
        Identity identity;

        var request = context.Request;
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

        if (request.Activated)
        {
            stream.Add(identity.Activate(registered.DateRegistered));
        }

        await _eventStore.SaveAsync(stream, cancellationToken);

        context.WithResponse(new()
        {
            Id = id.Value,
            Name = request.Name,
            RegisteredBy = request.AuditIdentityName,
            GeneratedPassword = request.GeneratedPassword,
            System = request.System
        });
    }
}