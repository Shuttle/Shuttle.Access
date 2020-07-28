using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
    [Route("api/[controller]")]
    public class SessionsController : Controller
	{
		private readonly ISessionService _sessionService;

		public SessionsController(ISessionService sessionService)
		{
			Guard.AgainstNull(sessionService, nameof(sessionService));

			_sessionService = sessionService;
		}

	    [HttpPost]
		public IActionResult Post([FromBody] RegisterSessionModel model)
		{
			Guard.AgainstNull(model, nameof(model));

		    if (string.IsNullOrEmpty(model.Username) ||
		        (string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.Token)))
		    {
                return Ok(new
                {
					Data = new {
                        Registered = false
					}
                });
            }

		    Guid.TryParse(model.Token, out var token);

			var registerSessionResult = _sessionService.Register(model.Username, model.Password, token);

			return registerSessionResult.Ok
				? Ok(new
			    {
                    Data = new
                    {
                        Registered = true,
                        Token = registerSessionResult.Token.ToString("n"),
                        Permissions = registerSessionResult.Permissions.Select(permission => new
                        {
                            Permission = permission
                        }).ToList()
                    }
				})
				: Ok(new
				{
                    Data = new
                    {
                        Registered = false
                    }
				});
		}
	}
}