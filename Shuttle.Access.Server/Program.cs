using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using Ninject;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Mediator;
using Shuttle.Core.Ninject;
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

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ServiceHost.Run<Host>();
        }
    }

    public class Host : IServiceHost
    {
        private IServiceBus _bus;
        private IKernel _kernel;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public void Start()
        {
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            Log.Information("[starting]");

            _kernel = new StandardKernel();

            var container = new NinjectComponentContainer(_kernel);

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

            if (!databaseContextFactory.IsAvailable("Access", _cancellationTokenSource.Token))
            {
                throw new ApplicationException("[connection failure]");
            }

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

            _cancellationTokenSource?.Cancel();

            _bus?.Dispose();

            Log.Information("[stopped]");
        }
    }
}