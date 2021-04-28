using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi
{
    [Route("[controller]")]
    public class SessionsController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISessionService _sessionService;

        public SessionsController(IDatabaseContextFactory databaseContextFactory, ISessionService sessionService,
            ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionService, nameof(sessionService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _databaseContextFactory = databaseContextFactory;
            _sessionService = sessionService;
            _sessionRepository = sessionRepository;
        }

        [HttpPost("request")]
        public IActionResult Post([FromBody] RegisterSessionRequestModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            if (string.IsNullOrEmpty(model.IdentityName))
            {
                return Ok(new
                {
                    Success = false
                });
            }

            var sessionTokenResult = HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return Unauthorized();
            }

            RegisterSessionResult registerSessionResult;

            using (_databaseContextFactory.Create())
            {
                registerSessionResult = _sessionService.Register(model.IdentityName, sessionTokenResult.SessionToken);
            }

            return registerSessionResult.Ok
                ? Ok(new
                {
                    Success = true,
                    registerSessionResult.IdentityName,
                    Token = registerSessionResult.Token.ToString("n"),
                    registerSessionResult.TokenExpiryDate,
                    Permissions = registerSessionResult.Permissions.Select(permission => new
                    {
                        Permission = permission
                    }).ToList()
                })
                : Ok(new
                {
                    Success = false
                });
        }


        [HttpPost]
        public IActionResult Post([FromBody] RegisterSessionModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            if (string.IsNullOrEmpty(model.IdentityName) ||
                string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.Token))
            {
                return Ok(new
                {
                    Success = false
                });
            }

            Guid.TryParse(model.Token, out var token);

            RegisterSessionResult registerSessionResult;

            using (_databaseContextFactory.Create())
            {
                registerSessionResult = _sessionService.Register(model.IdentityName, model.Password, token);
            }

            return registerSessionResult.Ok
                ? Ok(new
                {
                    Success = true,
                    registerSessionResult.IdentityName,
                    Token = registerSessionResult.Token.ToString("n"),
                    registerSessionResult.TokenExpiryDate,
                    Permissions = registerSessionResult.Permissions.Select(permission => new
                    {
                        Permission = permission
                    }).ToList()
                })
                : Ok(new
                {
                    Success = false
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