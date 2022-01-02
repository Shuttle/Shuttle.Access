using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class SessionsController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISessionQuery _sessionQuery;
        private readonly ISessionService _sessionService;

        public SessionsController(IDatabaseContextFactory databaseContextFactory, ISessionService sessionService,
            ISessionRepository sessionRepository, ISessionQuery sessionQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionService, nameof(sessionService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(sessionQuery, nameof(sessionQuery));

            _databaseContextFactory = databaseContextFactory;
            _sessionService = sessionService;
            _sessionRepository = sessionRepository;
            _sessionQuery = sessionQuery;
        }

        [HttpPost("delegated")]
        public IActionResult Post([FromBody] RegisterDelegatedSession message)
        {
            Guard.AgainstNull(message, nameof(message));

            if (string.IsNullOrEmpty(message.IdentityName))
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
                registerSessionResult = _sessionService.Register(message.IdentityName, sessionTokenResult.SessionToken);
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
        public IActionResult Post([FromBody] RegisterSession message)
        {
            Guard.AgainstNull(message, nameof(message));

            if (string.IsNullOrEmpty(message.IdentityName) ||
                string.IsNullOrEmpty(message.Password) && string.IsNullOrEmpty(message.Token))
            {
                return Ok(new
                {
                    Success = false
                });
            }

            Guid.TryParse(message.Token, out var token);

            RegisterSessionResult registerSessionResult;

            using (_databaseContextFactory.Create())
            {
                registerSessionResult = _sessionService.Register(message.IdentityName, message.Password, token);
            }

            return registerSessionResult.Ok
                ? Ok(new
                {
                    Success = true,
                    registerSessionResult.IdentityName,
                    registerSessionResult.Token,
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

        [RequiresPermission(Permissions.View.Sessions)]
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
                return _sessionService.Remove(sessionTokenResult.SessionToken)
                    ? Ok()
                    : Problem();
            }
        }

        [RequiresPermission(Permissions.View.Sessions)]
        [HttpGet("{token}")]
        public IActionResult Get(Guid token)
        {
            using (_databaseContextFactory.Create())
            {
                var session = _sessionQuery.Get(token);

                if (session == null)
                {
                    return BadRequest();
                }

                return Ok(session);
            }
        }

        [HttpGet("{id}/permissions")]
        [RequiresPermission(Permissions.View.Sessions)]
        public IActionResult GetPermission(Guid token)
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