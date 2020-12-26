using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi
{
    [Route("[controller]")]
    public class SessionsController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionService _sessionService;
        private readonly ISessionRepository _sessionRepository;

        public SessionsController(IDatabaseContextFactory databaseContextFactory, ISessionService sessionService, ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionService, nameof(sessionService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _databaseContextFactory = databaseContextFactory;
            _sessionService = sessionService;
            _sessionRepository = sessionRepository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegisterSessionModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            if (string.IsNullOrEmpty(model.Username) ||
                string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.Token))
            {
                return Ok(new
                {
                    Data = new
                    {
                        Registered = false
                    }
                });
            }

            Guid.TryParse(model.Token, out var token);

            RegisterSessionResult registerSessionResult;

            using (_databaseContextFactory.Create())
            {
                registerSessionResult = _sessionService.Register(model.Username, model.Password, token);
            }

            return registerSessionResult.Ok
                ? Ok(new
                {
                    Registered = true,
                    registerSessionResult.Username,
                    Token = registerSessionResult.Token.ToString("n"),
                    Permissions = registerSessionResult.Permissions.Select(permission => new
                    {
                        Permission = permission
                    }).ToList()
                })
                : Ok(new
                {
                    Registered = false
                });
        }

        [RequiresSession]
        [HttpDelete]
        public IActionResult Delete()
        {
            var sessionTokenResult = HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return BadRequest();
            }

            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Success = _sessionService.Remove(sessionTokenResult.SessionToken)
                });
            }
        }

        [HttpGet]
        public IActionResult Get(Guid token)
        {
            using (_databaseContextFactory.Create())
            {
                var session = _sessionRepository.Find(token);

                if (session == null)
                {
                    return BadRequest();
                }

                return Ok(new
                {

                    session.Permissions
                });
            }
        }
    }
}