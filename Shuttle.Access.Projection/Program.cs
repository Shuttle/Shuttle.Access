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
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

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
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            Log.Information("[starting]");

            _container = new WindsorContainer();

            var container = new WindsorComponentContainer(_container);

            container.RegisterSuffixed("Shuttle.Access.Sql");

            container.RegisterEventStore();

            _eventStore = container.Resolve<IEventStore>();

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

            Log.Information("[started]");
        }

        public void Stop()
        {
            Log.Information("[stopping]");

            _container?.Dispose();
            _eventProcessor?.Dispose();
            _eventStore?.AttemptDispose();

            Log.Information("[stopped]");
        }
    }
}