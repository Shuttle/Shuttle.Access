using Shuttle.Access.Events.User.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;

namespace Shuttle.Sentinel.Server.Projection
{
	public class UserProjection :
		IEventHandler<Registered>
	{
		private readonly IProjectionConfiguration _configuration;
		private readonly IDatabaseContextFactory _databaseContextFactory;
		private readonly ISystemUserQuery _query;

		public UserProjection(IProjectionConfiguration configuration, IDatabaseContextFactory databaseContextFactory,
			ISystemUserQuery query)
		{
			Guard.AgainstNull(configuration, nameof(configuration));
			Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
			Guard.AgainstNull(query, nameof(query));

			_configuration = configuration;
			_databaseContextFactory = databaseContextFactory;
			_query = query;
		}

		public void ProcessEvent(IEventHandlerContext<Registered> context)
		{
			using (_databaseContextFactory.Create(_configuration.EventProjectionProviderName, _configuration.EventProjectionConnectionString))
			{
				_query.Register(context.PrimitiveEvent, context.Event);
			}
		}
	}
}