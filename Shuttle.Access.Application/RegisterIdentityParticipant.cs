﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application;

public class RegisterIdentityParticipant : IParticipant<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdentityQuery _identityQuery;
    private readonly  IIdKeyRepository _idKeyRepository;
    private readonly IRoleQuery _roleQuery;

    public RegisterIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, IIdentityQuery identityQuery, IRoleQuery roleQuery)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(idKeyRepository);
        Guard.AgainstNull(identityQuery);
        Guard.AgainstNull(roleQuery);

        _eventStore = eventStore;
        _idKeyRepository = idKeyRepository;
        _identityQuery = identityQuery;
        _roleQuery = roleQuery;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<RegisterIdentity, IdentityRegistered>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message.Request;

        EventStream stream;
        Identity identity;

        var key = Identity.Key(message.Name);
        var id = await _idKeyRepository.FindAsync(key);

        if (id.HasValue)
        {
            identity = new();
            stream = await _eventStore.GetAsync(id.Value);

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

            await _idKeyRepository.AddAsync(id.Value, key);

            stream = await _eventStore.GetAsync(id.Value);
        }

        var registered = identity.Register(message.Name, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

        stream.Add(registered);

        var count = await _identityQuery.CountAsync(new DataAccess.Query.Identity.Specification().WithRoleName("Administrator"));

        if (count == 0)
        {
            var roles = (await _roleQuery.SearchAsync(new DataAccess.Query.Role.Specification().AddName("Administrator"))).ToList();

            if (roles.Count != 1)
            {
                throw new InvalidOperationException(Access.Resources.AdministratorRoleMissingException);
            }

            var role = roles[0];

            if (role.Name.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase))
            {
                stream.Add(identity.AddRole(role.Id));
            }
        }

        if (message.Activated)
        {
            stream.Add(identity.Activate(registered.DateRegistered));
        }

        context.Message.WithResponse(new()
        {
            Id = id.Value,
            Name = message.Name,
            RegisteredBy = message.RegisteredBy,
            GeneratedPassword = message.GeneratedPassword,
            System = message.System,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}