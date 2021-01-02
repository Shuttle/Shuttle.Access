using System;
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
        IMessageHandler<RegisterIdentityCommand>,
        IMessageHandler<SetIdentityRoleCommand>,
        IMessageHandler<RemoveIdentityCommand>,
        IMessageHandler<SetPasswordCommand>,
        IMessageHandler<ActivateIdentityCommand>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;

        public UserHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore,
            IIdentityQuery identityQuery, IRoleQuery roleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
            _identityQuery = identityQuery;
            _roleQuery = roleQuery;
        }

        public void ProcessMessage(IHandlerContext<RegisterIdentityCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));
            
            var message = context.Message;

            if (string.IsNullOrEmpty(message.Name) ||
                string.IsNullOrEmpty(message.RegisteredBy) ||
                message.PasswordHash == null ||
                message.PasswordHash.Length == 0)
            {
                return;
            }

            var id = Guid.NewGuid();

            using (_databaseContextFactory.Create())
            {
                var key = Identity.Key(message.Name);

                if (_keyStore.Contains(key))
                {
                    return;
                }

                var count = _identityQuery.Count(
                    new DataAccess.Query.Identity.Specification().WithRoleName("Administrator"));

                _keyStore.Add(id, key);

                var user = new Identity(id);
                var stream = _eventStore.CreateEventStream(id);

                var registered = user.Register(message.Name, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

                if (count == 0)
                {
                    var roles = _roleQuery
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

            context.Publish(new IdentityRegisteredEvent
            {
                Name = message.Name,
                RegisteredBy = message.RegisteredBy,
                GeneratedPassword = message.GeneratedPassword
            });
        }

        public void ProcessMessage(IHandlerContext<RemoveIdentityCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var user = new Identity(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(user);

                stream.AddEvent(user.Remove());

                _eventStore.Save(stream);

                _keyStore.Remove(message.Id);
            }
        }

        public void ProcessMessage(IHandlerContext<SetIdentityRoleCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var user = new Identity(message.IdentityId);
                var stream = _eventStore.Get(message.IdentityId);

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
                var user = new Identity(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(user);
                stream.AddEvent(user.SetPassword(message.PasswordHash));

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<ActivateIdentityCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var now = DateTime.Now;

            using (_databaseContextFactory.Create())
            {
                var user = new Identity(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(user);
                stream.AddEvent(user.Activate(now));

                _eventStore.Save(stream);
            }

            context.Publish(new IdentityActivatedEvent
            {
                Id = message.Id,
                DateActivated = now
            });
        }
    }
}