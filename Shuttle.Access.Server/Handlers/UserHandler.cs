using System;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Server
{
	public class UserHandler :
		IMessageHandler<RegisterUserCommand>,
		IMessageHandler<SetUserRoleCommand>,
        IMessageHandler<RemoveUserCommand>
	{
		private readonly IDatabaseContextFactory _databaseContextFactory;
		private readonly IEventStore _eventStore;
		private readonly IKeyStore _keyStore;
		private readonly ISystemUserQuery _systemUserQuery;

		public UserHandler(IDatabaseContextFactory databaseContextFactory, IEventStore eventStore, IKeyStore keyStore,
			ISystemUserQuery systemUserQuery)
		{
			Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
			Guard.AgainstNull(eventStore, nameof(eventStore));
			Guard.AgainstNull(keyStore, nameof(keyStore));
			Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));

			_databaseContextFactory = databaseContextFactory;
			_eventStore = eventStore;
			_keyStore = keyStore;
			_systemUserQuery = systemUserQuery;
		}

		public void ProcessMessage(IHandlerContext<RegisterUserCommand> context)
		{
			var message = context.Message;

			if (string.IsNullOrEmpty(message.Username))
			{
				return;
			}

			if (string.IsNullOrEmpty(message.RegisteredBy))
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

				var count = _systemUserQuery.Count();

				_keyStore.Add(id, key);

				var user = new User(id);
				var stream = _eventStore.CreateEventStream(id);

				var registered = user.Register(message.Username, message.PasswordHash, message.RegisteredBy);

				if (count == 0)
				{
					stream.AddEvent(user.AddRole("administrator"));
				}

				stream.AddEvent(registered);

				_eventStore.Save(stream);
			}
		}

		public void ProcessMessage(IHandlerContext<SetUserRoleCommand> context)
		{
			var message = context.Message;

			using (_databaseContextFactory.Create())
			{
				if (!message.Active && message.RoleName.Equals("administrator") && _systemUserQuery.AdministratorCount() == 1)
				{
					return;
				}

				var user = new User(message.UserId);
				var stream = _eventStore.Get(message.UserId);

				stream.Apply(user);

				if (message.Active && !user.IsInRole(message.RoleName))
				{
					stream.AddEvent(user.AddRole(message.RoleName));
				}

				if (!message.Active && user.IsInRole(message.RoleName))
				{
					stream.AddEvent(user.RemoveRole(message.RoleName));
				}

				_eventStore.Save(stream);
			}
		}

	    public void ProcessMessage(IHandlerContext<RemoveUserCommand> context)
	    {
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
	}
}