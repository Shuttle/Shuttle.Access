using System;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;
using Shuttle.Recall;
using Shuttle.Sentinel.Messages.v1;

namespace Shuttle.Sentinel.Server
{
    public class RoleHandler :
        IMessageHandler<AddRoleCommand>,
        IMessageHandler<RemoveRoleCommand>,
        IMessageHandler<SetRolePermissionCommand>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public RoleHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(eventStore, "eventStore");
            Guard.AgainstNull(keyStore, "keyStore");

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IHandlerContext<AddRoleCommand> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            using (_databaseContextFactory.Create())
            {
                var key = Role.Key(message.Name);

                if (_keyStore.Contains(key))
                {
                    return;
                }

                var id = Guid.NewGuid();

                _keyStore.Add(id, key);

                var role = new Role(id);
                var stream = _eventStore.CreateEventStream(id);

                stream.AddEvent(role.Add(message.Name));

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<SetRolePermissionCommand> context)
        {
            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var role = new Role(message.RoleId);
                var stream = _eventStore.Get(message.RoleId);

                stream.Apply(role);

                if (message.Active && !role.HasPermission(message.Permission))
                {
                    stream.AddEvent(role.AddPermission(message.Permission));
                }

                if (!message.Active && role.HasPermission(message.Permission))
                {
                    stream.AddEvent(role.RemovePermission(message.Permission));
                }

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<RemoveRoleCommand> context)
        {
            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var role = new Role(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(role);

                stream.AddEvent(role.Remove());

                _eventStore.Save(stream);
            }
        }
    }
}