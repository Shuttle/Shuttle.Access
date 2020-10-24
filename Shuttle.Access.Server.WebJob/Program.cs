using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Castle.Windsor;
using log4net;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Server.WebJob
{
    class Program
    {
        private static IServiceBus _bus;
        private static WindsorContainer _container;

        static async Task Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Program))));

            Log.Information("[starting]");

            _container = new WindsorContainer();

            var container = new WindsorComponentContainer(_container);

            container.RegisterSuffixed("Shuttle.Access.Sql");

            EventStore.Register(container);
            ServiceBus.Register(container);

            container.Resolve<IDatabaseContextFactory>().ConfigureWith("Access");

            _bus = ServiceBus.Create(container).Start();

            Log.Information("[started]");

            using (var host = new HostBuilder().UseConsoleLifetime().Build())
            {
                await host.RunAsync();
            }

            Log.Information("[stopping]");

            _bus?.Dispose();
            _container?.Dispose();

            Log.Information("[stopped]");
        }
    }
}