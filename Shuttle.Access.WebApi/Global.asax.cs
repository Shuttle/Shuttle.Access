using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net;
using Newtonsoft.Json.Serialization;
using Shuttle.Access.WebApi.ApplicationStart;
using Shuttle.Core.Castle;
using Shuttle.Core.Castle.Extensions;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Infrastructure;
using Shuttle.Core.Log4Net;
using Shuttle.Esb;
using Shuttle.Recall;
using ILog = Shuttle.Core.Infrastructure.ILog;

namespace Shuttle.Access.WebApi
{
    public class WebApiApplication : HttpApplication
    {
        private static Exception _startupException;
        private static ILog _log;
        private static IServiceBus _serviceBus;
        private static IWindsorContainer _container;
        private static IEventStore _eventStore;

        protected void Application_Start()
        {
            try
            {
                Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(WebApiApplication))));

                _log = Log.For(this);

                _log.Information("[starting]");

                new ConnectionStringService().Approve();

                ConfigureWindsorContainer();

                var container = new WindsorComponentContainer(_container);

                ServiceBus.Register(container);
                EventStore.Register(container);

                _serviceBus = ServiceBus.Create(container).Start();
                _eventStore = EventStore.Create(container);

                RequiresPermissionAttribute.Assign(_container);
                RequiresSessionAttribute.Assign(_container);

                GlobalConfiguration.Configuration.DependencyResolver = new ApiResolver(_container);

                _container.Register(
                    Component.For<IHttpControllerActivator>().Instance(new ApiControllerActivator(_container)));

                ConfigureJson(GlobalConfiguration.Configuration);

                WebApiConfiguration.Register(GlobalConfiguration.Configuration);

                GlobalConfiguration.Configuration.EnsureInitialized();

                container.Resolve<IDatabaseContextFactory>().ConfigureWith("Access");

                _log.Information("[started]");
            }
            catch (Exception ex)
            {
                _log.Fatal("[could not start]");
                _startupException = ex;
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (_startupException == null)
            {
                return;
            }

            var html = new StringBuilder();

            html.Append("<html><head><title>Shuttle Access Web-Api Startup Exception</title><style>");

            html.Append(
                "body { background: none repeat scroll 0 0 #CBE1EF; font-family: 'MS Tresbuchet',Verdana,Arial,Helvetica,sans-serif; font-size: 0.7em; margin: 0; }");
            html.Append(
                "div.header { background-color: #5c87b2; color: #ffffff; padding: .2em 2%; font-size: 2em; font-weight: bold; }");
            html.Append(
                "div.error { border: solid 1px #5a7fa5; background-color: #ffffff; color: #CC0000; padding: .5em; font-size: 2em; width: 96%; margin: .5em auto; }");
            html.Append(
                "div.information { border: solid 1px #5a7fa5; background-color: #ffffff; color: #555555; padding: 1em; font-size: 1em; width: 96%; margin: 1em auto; }");

            html.Append("</style></head><body>");
            html.Append("<div class='header'>Shuttle Access Web-Api Startup Exception</div>");
            html.AppendFormat("<div class='error'><b>source</b>:<br>{0}</div>", _startupException.Source);
            html.AppendFormat("<div class='information'><b>message</b>:<br>{0}</div>", _startupException);

            var crlf = new Regex(@"(\r\n|\r|\n)+");

            html.AppendFormat("<div class='information'><b>stack trace</b>:<br>{0}</div>",
                crlf.Replace(_startupException.StackTrace, "<br/>"));

            var reflection = _startupException as ReflectionTypeLoadException;

            if (reflection != null)
            {
                html.Append("<div class='information'><b>loader exception(s)</b>:<br>");

                foreach (var exception in reflection.LoaderExceptions)
                {
                    html.AppendFormat("{0}<br/>", exception);

                    var file = exception as FileNotFoundException;

                    if (file == null)
                    {
                        return;
                    }

                    html.Append("[fusion log follows]<br/>");
                    html.AppendFormat("{0}<br/>", file.FusionLog);
                }

                html.Append("</div>");
            }

            html.Append("</body></html>");

            HttpContext.Current.Response.Write(html);
            HttpContext.Current.Response.End();
        }

        private void Application_End(object sender, EventArgs e)
        {
            _log.Information("[stopping]");

            _serviceBus?.Dispose();
            _container?.Dispose();
            _eventStore?.AttemptDispose();

            _log.Information("[stopped]");

            LogManager.Shutdown();
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var encodingValue = context.Response.Headers["Content-Encoding"];

            if (encodingValue == "gzip" || encodingValue == "deflate")
            {
                context.Response.Headers.Remove("Content-Encoding");
                context.Response.Filter = null;
            }

            _log.Error(Server.GetLastError().ToString());

            var reflection = Server.GetLastError() as ReflectionTypeLoadException;

            if (reflection == null)
            {
                return;
            }

            foreach (var exception in reflection.LoaderExceptions)
            {
                _log.Error($"- '{exception.Message}'.");

                var file = exception as FileNotFoundException;

                if (file == null)
                {
                    return;
                }

                _log.Error("[fusion log follows]:");
                _log.Error(file.FusionLog);
            }
        }

        private static void ConfigureJson(HttpConfiguration configuration)
        {
            configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        }

        private void ConfigureWindsorContainer()
        {
            _container = new WindsorContainer();

            _container.RegisterDataAccess("Shuttle.Access.Sql");

            _container.Register(Component.For<IDatabaseContextCache>().ImplementedBy<ContextDatabaseContextCache>());
            _container.Register("Shuttle.Access.WebApi", typeof(ApiController), "Controller");
            _container.Register("Shuttle.Access.Sql", "Service");
        }
    }
}