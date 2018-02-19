using System;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
	public class SessionsController : Controller
	{
		private readonly ISessionService _sessionService;

		public SessionsController(ISessionService sessionService)
		{
			Guard.AgainstNull(sessionService, nameof(sessionService));

			_sessionService = sessionService;
		}

	    [HttpPost("api/sessions/")]
		public IActionResult Post([FromBody] RegisterSessionModel model)
		{
			Guard.AgainstNull(model, nameof(model));

		    if (string.IsNullOrEmpty(model.Username) ||
		        (string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.Token)))
		    {
                return Ok(new
                {
                    Registered = false
                });
            }

		    Guid token;

		    Guid.TryParse(model.Token, out token);

			var registerSessionResult = _sessionService.Register(model.Username, model.Password, token);

			return registerSessionResult.Ok
				? Ok(new
			    {
			        Registered = true,
			        Token = registerSessionResult.Token.ToString("n"),
			        registerSessionResult.Permissions
			    })
				: Ok(new
				{
					Registered = false
				});
		}
	}
}