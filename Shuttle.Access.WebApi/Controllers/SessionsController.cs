using System;
using System.Web.Http;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.WebApi
{
	public class SessionsController : ApiController
	{
		private readonly ISessionService _sessionService;

		public SessionsController(ISessionService sessionService)
		{
			Guard.AgainstNull(sessionService, "sessionService");

			_sessionService = sessionService;
		}

		public IHttpActionResult Post([FromBody] RegisterSessionModel model)
		{
			Guard.AgainstNull(model, "model");

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
				? (IHttpActionResult) Ok(new
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