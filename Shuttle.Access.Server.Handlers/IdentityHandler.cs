using System;
using System.Linq;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server.Handlers
{
    public class IdentityHandler :
        IMessageHandler<RegisterIdentity>,
        IMessageHandler<SetIdentityRoleStatus>,
        IMessageHandler<RemoveIdentity>,
        IMessageHandler<SetPassword>,
        IMessageHandler<ActivateIdentity>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IKeyStore _keyStore;
        private readonly IRoleQuery _roleQuery;
        private readonly IMediator _mediator;
        private readonly IIdentityQuery _identityQuery;

        public IdentityHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore,
            IIdentityQuery identityQuery, IRoleQuery roleQuery, IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(keyStore, nameof(keyStore));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _eventStore = eventStore;
            _keyStore = keyStore;
            _identityQuery = identityQuery;
            _roleQuery = roleQuery;
            _mediator = mediator;
        }

        public void ProcessMessage(IHandlerContext<RegisterIdentity> context)
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

            using (_databaseContextFactory.Create())
            {
                EventStream stream;
                Identity identity;

                var key = Identity.Key(message.Name);
                var id = _keyStore.Get(key);

                if (id.HasValue)
                {
                    identity = new Identity(id.Value);
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
                    identity = new Identity(id.Value);

                    _keyStore.Add(id.Value, key);

                    stream = _eventStore.CreateEventStream(id.Value);
                }

                var registered = identity.Register(message.Name, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

                var count = _identityQuery.Count(
                    new DataAccess.Query.Identity.Specification().WithRoleName("Administrator"));

                if (count == 0)
                {
                    var roles = _roleQuery
                        .Search(new DataAccess.Query.Role.Specification().WithRoleName("Administrator")).ToList();

                    if (roles.Count != 1)
                    {
                        context.Send(new AddRole
                        {
                            Name = "Administrator"
                        }, c => c.Local());

                        throw new InvalidOperationException(Resources.AdministratorRoleMissingException);
                    }

                    var role = roles[0];

                    if (role.RoleName.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase))
                    {
                        stream.AddEvent(identity.AddRole(role.Id));
                    }
                }

                stream.AddEvent(registered);

                if (message.Activated)
                {
                    stream.AddEvent(identity.Activate(registered.DateRegistered));
                }

                _eventStore.Save(stream);

                context.Publish(new IdentityRegistered
                {
                    Name = message.Name,
                    RegisteredBy = message.RegisteredBy,
                    GeneratedPassword = message.GeneratedPassword,
                    System = message.System
                });
            }
        }

        public void ProcessMessage(IHandlerContext<RemoveIdentity> context)
        {
            using (_databaseContextFactory.Create())
            {
                _mediator.Send(context.Message);

                context.Publish(new IdentityRemoved
                {
                    Id = context.Message.Id
                });
            }
        }

        public void ProcessMessage(IHandlerContext<SetIdentityRoleStatus> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(message);

                _mediator.Send(reviewRequest);

                if (!reviewRequest.Ok)
                {
                    return;
                }

                _mediator.Send(message);
            }
        }

        public void ProcessMessage(IHandlerContext<SetPassword> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                var identity = new Identity(message.Id);
                var stream = _eventStore.Get(message.Id);

                stream.Apply(identity);
                stream.AddEvent(identity.SetPassword(message.PasswordHash));

                _eventStore.Save(stream);
            }
        }

        public void ProcessMessage(IHandlerContext<ActivateIdentity> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;
            var now = DateTime.Now;

            var specification = new DataAccess.Query.Identity.Specification();

            if (message.Id.HasValue)
            {
                specification.WithIdentityId(message.Id.Value);
            }
            else
            {
                specification.WithName(message.Name);
            }

            Guid id;

            using (_databaseContextFactory.Create())
            {
                var query = _identityQuery.Search(specification).FirstOrDefault();

                if (query == null)
                {
                    return;
                }

                id = query.Id;

                var identity = new Identity(id);
                var stream = _eventStore.Get(id);

                stream.Apply(identity);
                stream.AddEvent(identity.Activate(now));

                _eventStore.Save(stream);
            }

            context.Publish(new IdentityActivated
            {
                Id = id,
                DateActivated = now
            });
        }
    }
}