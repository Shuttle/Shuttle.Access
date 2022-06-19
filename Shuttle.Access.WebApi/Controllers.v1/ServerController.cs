using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.WebApi.Controllers.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class ServerController : Controller
    {
        [HttpGet("configuration")]
        public IActionResult GetServerConfiguration()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return Ok(new ServerConfiguration { Version = $"{version.Major}.{version.Minor}.{version.Build}" });
        }
    }
}