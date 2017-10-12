using System.Reflection;
using System.Web.Http;

namespace Shuttle.Access.WebApi
{
    public class ServerController : ApiController
    {
        [HttpGet]
        [Route("api/server/configuration")]
        public IHttpActionResult GetServerConfiguration()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return Ok(new
            {
                Version = $"{version.Major}.{version.Minor}.{version.Build}"
            });
        }
    }
}