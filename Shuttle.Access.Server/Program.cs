using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Castle.Windsor;
using log4net;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Container;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Mediator;
using Shuttle.Core.ServiceHost;
using Shuttle.Esb;
using Shuttle.Esb.AzureMQ;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server
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
        private IServiceBus _bus;
        private WindsorContainer _container;

        public void Start()
        {
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));
            
            Log.Information("[starting]");

            _container = new WindsorContainer();

            var container = new WindsorComponentContainer(_container);

            container.Register<IAzureStorageConfiguration, DefaultAzureStorageConfiguration>();

            container.RegisterDataAccess();
            container.RegisterSuffixed("Shuttle.Access.Sql");
            container.RegisterEventStore();
            container.RegisterEventStoreStorage();
            container.RegisterSubscription();
            container.RegisterServiceBus();
            container.RegisterMessageHandlers(Assembly.Load("Shuttle.Access.Server.Handlers"));
            container.RegisterMediator();
            container.RegisterMediatorParticipants(Assembly.Load("Shuttle.Access.Application"));
            container.Register<IHashingService, HashingService>();

            var databaseContextFactory = container.Resolve<IDatabaseContextFactory>().ConfigureWith("Access");

            _bus = container.Resolve<IServiceBus>().Start();

            using (databaseContextFactory.Create())
            {
                container.Resolve<IMediator>().Send(new ConfigureApplication());
            }

            Log.Information("[started]");
        }

        public void Stop()
        {
            Log.Information("[stopping]");
            
            _bus?.Dispose();
            
            Log.Information("[stopped]");
        }
    }
}