﻿using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server.Handlers
{
    public class UserHandler :
        IMessageHandler<RegisterUserCommand>,
        IMessageHandler<SetUserRoleCommand>,
        IMessageHandler<RemoveUserCommand>,
        IMessageHandler<SetPasswordCommand>,
        IMessageHandler<ActivateUserCommand>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDataRowMapper _dataRowMapper;
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public UserHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore,
            ISystemUserQuery systemUserQuery, ISystemRoleQuery systemRoleQuery, IDataRowMapper dataRowMapper)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));
            Guard.AgainstNull(dataRowMapper, nameof(dataRowMapper));

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
            _systemUserQuery = systemUserQuery;
            _systemRoleQuery = systemRoleQuery;
            _dataRowMapper = dataRowMapper;
        }

        public void ProcessMessage(IHandlerContext<RegisterUserCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));
            
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Username) ||
                string.IsNullOrEmpty(message.RegisteredBy) ||
                message.PasswordHash == null ||
                message.PasswordHash.Length == 0)
            {
                return;
            }

            var id = Guid.NewGuid();

            using (_databaseContextFactory.Create())
            {
                var key = User.Key(message.Username);

                if (_keyStore.Contains(key))
                {
                    return;
                }

                var count = _systemUserQuery.Count(
                    new DataAccess.Query.User.Specification().WithRoleName("Administrator"));

                _keyStore.Add(id, key);

                var user = new User(id);
                var stream = _eventStore.CreateEventStream(id);

                var registered = user.Register(message.Username, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

                if (count == 0)
                {
                    var roles = _systemRoleQuery
                        .Search(new DataAccess.Query.Role.Specification().WithRoleName("Administrator")).ToList();

                    if (roles.Count != 1)
                    {
                        context.Send(new AddRoleCommand
                        {
                            Name = "Administrator"
                        }, c => c.Local());

                        throw new InvalidOperationException(Resources.AdministratorRoleMissingException);
                    }

                    var role = roles[0];

                    if (role.RoleName.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase))
                    {
                        stream.AddEvent(user.AddRole(role.Id));
                    }
                }

                stream.AddEvent(registered);

                _eventStore.Save(stream);
            }

            context.Publish(new UserRegisteredEvent
            {
                Username = message.Username,
                RegisteredBy = message.RegisteredBy,
                GeneratedPassword = message.GeneratedPassword
            });
        }

        public void ProcessMessage(IHandlerContext<RemoveUserCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var user = new User(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(user);

                stream.AddEvent(user.Remove());

                _eventStore.Save(stream);

                _keyStore.Remove(message.Id);
            }
        }

        public void ProcessMessage(IHandlerContext<SetUserRoleCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var user = new User(message.UserId);
                var stream = _eventStore.Get(message.UserId);

                stream.Apply(user);

                if (message.Active && !user.IsInRole(message.RoleId))
                {
                    stream.AddEvent(user.AddRole(message.RoleId));
                }

                if (!message.Active && user.IsInRole(message.RoleId))
                {
                    stream.AddEvent(user.RemoveRole(message.RoleId));
                }

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<SetPasswordCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var user = new User(message.UserId);
                var stream = _eventStore.Get(message.UserId);

                stream.Apply(user);
                stream.AddEvent(user.SetPassword(message.PasswordHash));

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<ActivateUserCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var now = DateTime.Now;

            using (_databaseContextFactory.Create())
            {
                var user = new User(message.UserId);
                var stream = _eventStore.Get(message.UserId);

                stream.Apply(user);
                stream.AddEvent(user.Activate(now));

                _eventStore.Save(stream);
            }

            context.Publish(new UserActivatedEvent
            {
                UserId = message.UserId,
                DateActivated = now
            });
        }
    }
}