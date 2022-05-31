using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using log4net;
using Ninject;
using Shuttle.Access.Projection.Handlers;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Ninject;
using Shuttle.Core.Reflection;
using Shuttle.Core.ServiceHost;
using Shuttle.Core.Threading;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Projection
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
        private IEventProcessor _eventProcessor;
        private IEventStore _eventStore;
        private IKernel _kernel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void Start()
        {
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            Log.Information("[starting]");

            _kernel = new StandardKernel();

            var container = new NinjectComponentContainer(_kernel);

            container.RegisterDataAccess();
            container.RegisterSuffixed("Shuttle.Access.Sql");
            container.RegisterEventStore();
            container.RegisterEventStoreStorage();
            container.RegisterEventProcessing();

            _ = container.Resolve<EventProcessingModule>();

            _eventStore = container.Resolve<IEventStore>();

            _eventProcessor = container.Resolve<IEventProcessor>();

            var databaseContextFactory = container.Resolve<IDatabaseContextFactory>();

            if (!databaseContextFactory.IsAvailable("Access", _cancellationTokenSource.Token))
            {
                throw new ApplicationException("[connection failure]");
            }

            using (databaseContextFactory.Create("Access"))
            {
                _eventProcessor.AddProjection("Identity");
                _eventProcessor.AddProjection("Permission");
                _eventProcessor.AddProjection("Role");

                container.AddEventHandler<IdentityHandler>("Identity");
                container.AddEventHandler<PermissionHandler>("Permission");
                container.AddEventHandler<RoleHandler>("Role");
            }

            _eventProcessor.Start();

            Log.Information("[started]");
        }

        public void Stop()
        {
            Log.Information("[stopping]");

            _cancellationTokenSource?.Cancel();

            _kernel?.Dispose();
            _eventProcessor?.Dispose();
            _eventStore?.AttemptDispose();

            Log.Information("[stopped]");
        }
    }
}