using Castle.Windsor;
using log4net;
using Shuttle.Core.Castle;
using Shuttle.Core.Castle.Extensions;
using Shuttle.Core.Infrastructure;
using Shuttle.Core.Log4Net;
using Shuttle.Core.ServiceHost;
using Shuttle.Recall;

namespace Shuttle.Access.Server.Projection
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
            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            _container = new WindsorContainer();

            _container.RegisterDataAccess("Shuttle.Access.Sql");

            var container = new WindsorComponentContainer(_container);

            EventStore.Register(container);

            _eventStore = EventStore.Create(container);

            container.Register<UserHandler>();
            container.Register<RoleHandler>();

            _eventProcessor = container.Resolve<IEventProcessor>();

            _eventProcessor.AddProjection(
                new Recall.Projection("SystemUsers").AddEventHandler(container.Resolve<UserHandler>()));
            _eventProcessor.AddProjection(
                new Recall.Projection("SystemRoles").AddEventHandler(container.Resolve<RoleHandler>()));

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