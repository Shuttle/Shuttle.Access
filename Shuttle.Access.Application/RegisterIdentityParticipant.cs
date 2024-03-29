﻿
using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Application
{
    public class RegisterIdentityParticipant : IParticipant<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>
    {
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;

        public RegisterIdentityParticipant(IEventStore eventStore, IKeyStore keyStore, IIdentityQuery identityQuery, IRoleQuery roleQuery)
        {
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));

            _eventStore = eventStore;
            _keyStore = keyStore;
            _identityQuery = identityQuery;
            _roleQuery = roleQuery;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<RegisterIdentity, IdentityRegistered>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message.Request;

            EventStream stream;
            Identity identity;

            var key = Identity.Key(message.Name);
            var id = _keyStore.Get(key);

            if (id.HasValue)
            {
                identity = new Identity();
                stream = _eventStore.Get(id.Value);

                stream.Apply(identity);

                if (!identity.Removed)
                {
                    return;
                }
            }
            else
            {
                id = Guid.NewGuid();
                identity = new Identity();

                _keyStore.Add(id.Value, key);

                stream = _eventStore.CreateEventStream(id.Value);
            }

            var registered = identity.Register(message.Name, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

            var count = _identityQuery.Count(
                new DataAccess.Query.Identity.Specification().WithRoleName("Administrator"));

            if (count == 0)
            {
                var roles = _roleQuery
                    .Search(new DataAccess.Query.Role.Specification().AddName("Administrator")).ToList();

                if (roles.Count != 1)
                {
                    throw new InvalidOperationException(Access.Resources.AdministratorRoleMissingException);
                }

                var role = roles[0];

                if (role.Name.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase))
                {
                    stream.AddEvent(identity.AddRole(role.Id));
                }
            }

            stream.AddEvent(registered);

            if (message.Activated)
            {
                stream.AddEvent(identity.Activate(registered.DateRegistered));
            }

            context.Message.WithResponse(new IdentityRegistered
            {
                Id = id.Value,
                Name = message.Name,
                RegisteredBy = message.RegisteredBy,
                GeneratedPassword = message.GeneratedPassword,
                System = message.System,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}