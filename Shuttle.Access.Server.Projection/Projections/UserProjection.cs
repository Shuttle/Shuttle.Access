using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer;
using Shuttle.Sentinel.DomainEvents.User.v1;

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
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
			Guard.AgainstNull(query, "query");

			_configuration = configuration;
			_databaseContextFactory = databaseContextFactory;
			_query = query;
		}

		public void ProcessEvent(IEventHandlerContext<Registered> context)
		{
			using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
			{
				_query.Register(context.ProjectionEvent, context.DomainEvent);
			}
		}
	}
}