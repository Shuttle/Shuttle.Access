using System.Data.Common;
using System.Data.SqlClient;
using Castle.Windsor;
using log4net;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Container;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.ServiceHost;
using Shuttle.Esb;
using Shuttle.Recall;

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

            container.RegisterSuffixed("Shuttle.Access.Sql");

            EventStore.Register(container);
            ServiceBus.Register(container);

            container.Resolve<IDatabaseContextFactory>().ConfigureWith("Access");

            _bus = ServiceBus.Create(container).Start();

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