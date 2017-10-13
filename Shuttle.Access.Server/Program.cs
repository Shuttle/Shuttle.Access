using System.Net;
using System.Net.Sockets;
using Castle.Windsor;
using log4net;
using Shuttle.Core.Castle;
using Shuttle.Core.Castle.Extensions;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Core.Log4Net;
using Shuttle.Core.ServiceHost;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.Server
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
        private IServiceBus _bus;
        private WindsorContainer _container;

        public void Start()
        {
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            _container = new WindsorContainer();

            _container.RegisterDataAccess("Shuttle.Access.Sql");

            var container = new WindsorComponentContainer(_container);

            EventStore.Register(container);
            ServiceBus.Register(container);

            container.Resolve<IDatabaseContextFactory>().ConfigureWith("Access");

            _bus = ServiceBus.Create(container).Start();
        }

        public void Stop()
        {
            _bus?.Dispose();
        }
    }
}