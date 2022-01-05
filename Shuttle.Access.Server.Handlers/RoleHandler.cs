using System;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server.Handlers
{
    public class RoleHandler :
        IMessageHandler<AddRole>,
        IMessageHandler<RemoveRole>,
        IMessageHandler<SetRolePermissionStatus>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;

        public RoleHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
        }

        public void ProcessMessage(IHandlerContext<AddRole> context)
        {
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name))
            {
                return;
            }

            Guid id;

            using (_databaseContextFactory.Create())
            {
                var key = Role.Key(message.Name);

                if (_keyStore.Contains(key))
                {
                    return;
                }

                id = Guid.NewGuid();

                _keyStore.Add(id, key);

                var role = new Role(id);
                var stream = _eventStore.CreateEventStream(id);

                stream.AddEvent(role.Add(message.Name));

                _eventStore.Save(stream);
            }

            context.Publish(new RoleAdded
            {
                Id = id,
                Name = message.Name
            });
        }

        public void ProcessMessage(IHandlerContext<RemoveRole> context)
        {
            var message = context.Message;

            Role role;

            using (_databaseContextFactory.Create())
            {
                role = new Role(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(role);

                stream.AddEvent(role.Remove());

                _eventStore.Save(stream);
            }

            context.Publish(new RoleRemoved
            {   
                Id = message.Id,
                Name = role.Name
            });
        }

        public void ProcessMessage(IHandlerContext<SetRolePermissionStatus> context)
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

            context.Publish(new RolePermissionStatusSet
            {
                RoleId = message.RoleId,
                Permission = message.Permission,
                Active = message.Active
            });
        }
    }
}