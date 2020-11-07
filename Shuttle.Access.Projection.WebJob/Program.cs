using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Castle.Windsor;
using log4net;
using Microsoft.Extensions.Hosting;
using Shuttle.Access.Projection.Handlers;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;
using Shuttle.Recall;

namespace Shuttle.Access.Projection.WebJob
{
    class Program
    {
        private static WindsorContainer _container;
        private static IEventProcessor _eventProcessor;
        private static IEventStore _eventStore;

        static async Task Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Log.Assign(
                new Log4NetLog(LogManager.GetLogger(typeof(Program)),
                    new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? ".", "log4net.xml"))));

            Log.Information("[starting]"); 
            
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
                _eventProcessor.AddProjection("SystemUsers");
                _eventProcessor.AddProjection("SystemRoles");

                container.AddEventHandler<UserHandler>("SystemUsers");
                container.AddEventHandler<RoleHandler>("SystemRoles");
            }

            _eventProcessor.Start();

            Log.Information("[started]");
            
            using (var host = new HostBuilder().UseConsoleLifetime().Build())
            {
                await host.RunAsync();
            }

            Log.Information("[stopping]");

            _eventProcessor?.Dispose();
            _eventStore?.AttemptDispose();
            _container?.Dispose();

            Log.Information("[stopped]");
        }
    }
}