using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Castle.Windsor;
using log4net;
using Shuttle.Access.Projection.Handlers;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;
using Shuttle.Core.ServiceHost;
using Shuttle.Recall;

namespace Shuttle.Access.Projection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceHost.Run<Host>();
        }
    }

    public class Host : IServiceHost
    {
        private IWindsorContainer _container;
        private IEventProcessor _eventProcessor;
        private IEventStore _eventStore;

        public void Start()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));
            
            Log.Information(ConfigurationManager.ConnectionStrings["Access"].ConnectionString);

            _container = new WindsorContainer();

            var container = new WindsorComponentContainer(_container);

            container.RegisterSuffixed("Shuttle.Access.Sql");

            EventStore.Register(container);

            _eventStore = EventStore.Create(container);

            container.Register<IdentityHandler>();
            container.Register<RoleHandler>();

            _eventProcessor = container.Resolve<IEventProcessor>();

            using (container.Resolve<IDatabaseContextFactory>().Create("Access"))
            {
                _eventProcessor.AddProjection("Identity");
                _eventProcessor.AddProjection("Role");
                
                container.AddEventHandler<IdentityHandler>("Identity");
                container.AddEventHandler<RoleHandler>("Role");
            }

            _eventProcessor.Start();
        }

        public void Stop()
        {
            _container?.Dispose();
            _eventProcessor?.Dispose();
            _eventStore?.AttemptDispose();
        }
    }
}