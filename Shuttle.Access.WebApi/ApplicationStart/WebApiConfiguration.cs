using System.Web.Http;

namespace Shuttle.Access.WebApi.ApplicationStart
{
    public static class WebApiConfiguration
    {
        public static void Register(HttpConfiguration configuration)
        {
            configuration.MapHttpAttributeRoutes();
            configuration.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            configuration.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new {id = RouteParameter.Optional});

            GlobalConfiguration.Configuration.EnsureInitialized();
        }
    }
}