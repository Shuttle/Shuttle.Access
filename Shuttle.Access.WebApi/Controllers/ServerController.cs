using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Shuttle.Access.WebApi
{
    public class ServerController : Controller
    {
        [HttpGet("server/configuration")]
        public IActionResult GetServerConfiguration()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return Ok(new
            {
                Version = $"{version.Major}.{version.Minor}.{version.Build}"
            });
        }
    }
}