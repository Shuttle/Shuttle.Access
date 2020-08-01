using System.Data.Common;
using System.Data.SqlClient;
using Castle.Windsor;
using log4net;
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
#if NETCOREAPP
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif

            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            _container = new WindsorContainer();

            var container = new WindsorComponentContainer(_container);

            container.RegisterSuffixed("Shuttle.Access.Sql");

            EventStore.Register(container);

            _eventStore = EventStore.Create(container);

            container.Register<UserHandler>();
            container.Register<RoleHandler>();

            _eventProcessor = container.Resolve<IEventProcessor>();

            using (container.Resolve<IDatabaseContextFactory>().Create("Access"))
            {
                container.AddEventHandler<UserHandler>("SystemUsers");
                container.AddEventHandler<RoleHandler>("SystemRoles");
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