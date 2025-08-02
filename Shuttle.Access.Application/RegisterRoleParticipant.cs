using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RegisterRoleParticipant : IParticipant<RequestResponseMessage<RegisterRole, RoleRegistered>>
{
    private readonly IPermissionQuery _permissionQuery;
    private readonly IEventStore _eventStore;
    private readonly IIdKeyRepository _idKeyRepository;

    public RegisterRoleParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IPermissionQuery permissionQuery)
    {
        _eventStore = Guard.AgainstNull(eventStore);
        _idKeyRepository = Guard.AgainstNull(idKeyRepository);
        _permissionQuery = Guard.AgainstNull(permissionQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterRole, RoleRegistered>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message.Request;

        var permissionIds = new List<Guid>();

        foreach (var permission in message.GetPermissions())
        {
            var permissionId = (await _permissionQuery.SearchAsync(new DataAccess.Permission.Specification().AddName(permission.Name))).FirstOrDefault()?.Id;

            if (permissionId.HasValue)
            {
                permissionIds.Add(permissionId.Value);
            }
            else
            {
                message.MissingPermissions();
                return;
            }
        }

        var key = Role.Key(message.Name);

        if (await _idKeyRepository.ContainsAsync(key))
        {
            return;
        }

        var id = Guid.NewGuid();

        await _idKeyRepository.AddAsync(id, key);

        var role = new Role();
        var stream = await _eventStore.GetAsync(id);

        stream.Add(role.Register(message.Name));

        foreach (var permissionId in permissionIds)
        {
            if (!role.HasPermission(permissionId))
            {
                stream.Add(role.AddPermission(permissionId));
            }
        }

        context.Message.WithResponse(new()
        {
            Id = id,
            Name = message.Name,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}