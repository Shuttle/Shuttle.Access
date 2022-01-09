using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Recall;
using SetIdentityRoleStatus = Shuttle.Access.Messages.v1.SetIdentityRoleStatus;

namespace Shuttle.Access.WebApi.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class IdentitiesController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceBus _serviceBus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly ISessionRepository _sessionRepository;
        private readonly IIdentityQuery _identityQuery;
        private readonly IAccessService _accessService;
        private readonly IMediator _mediator;

        public IdentitiesController(IDatabaseContextFactory databaseContextFactory, IServiceBus serviceBus,
            IHashingService hashingService, ISessionRepository sessionRepository,
            IAuthenticationService authenticationService, IAuthorizationService authorizationService,
            IIdentityQuery identityQuery, IEventStore eventStore,
            IPasswordGenerator passwordGenerator, IAccessService accessService, IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(passwordGenerator, nameof(passwordGenerator));
            Guard.AgainstNull(accessService, nameof(accessService));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _serviceBus = serviceBus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _identityQuery = identityQuery;
            _eventStore = eventStore;
            _passwordGenerator = passwordGenerator;
            _accessService = accessService;
            _mediator = mediator;
        }

        [HttpGet]
        [RequiresPermission(Permissions.View.Identity)]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_identityQuery.Search(new DataAccess.Query.Identity.Specification()));
            }
        }

        
        [HttpGet("{value}")]
        [RequiresPermission(Permissions.View.Identity)]
        public IActionResult Get(string value)
        {
            using (_databaseContextFactory.Create())
            {
                var specification = new DataAccess.Query.Identity.Specification().IncludeRoles();

                if (Guid.TryParse(value, out var id))
                {
                    specification.WithIdentityId(id);
                }
                else
                {
                    specification.WithName(value);
                }

                var user = _identityQuery.Search(specification).SingleOrDefault();

                return user != null
                    ? Ok(user)
                    : BadRequest();
            }
        }

        [HttpDelete("{id}")]
        [RequiresPermission(Permissions.Remove.Identity)]
        public IActionResult Delete(Guid id)
        {
            _serviceBus.Send(new RemoveIdentityCommand
            {
                Id = id
            });

            return Accepted();
        }

        [HttpPost("setrolestatus")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult SetRole([FromBody] SetIdentityRoleStatus message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                var reviewRequest = new ReviewRequest<SetIdentityRoleStatus>(message);

                _mediator.Send(reviewRequest);

                if (!reviewRequest.Ok)
                {
                    return BadRequest(reviewRequest.Message);
                }
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpPost("changepassword")]
        public IActionResult ChangePassword([FromBody] ChangePassword message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var sessionTokenResult = HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return BadRequest(Access.Resources.SessionTokenException);
            }

            var passwordHash = _hashingService.Sha256(message.NewPassword);

            using (_databaseContextFactory.Create())
            {
                var session = _sessionRepository.Find(sessionTokenResult.SessionToken);

                if (session == null)
                {
                    return BadRequest(Access.Resources.SessionTokenException);
                }

                if (!string.IsNullOrWhiteSpace(message.OldPassword) && string.IsNullOrWhiteSpace(message.Token))
                {
                    var authenticationResult = _authenticationService.Authenticate(session.IdentityName, message.OldPassword);

                    if (!authenticationResult.Authenticated)
                    {
                        return BadRequest(Access.Resources.InvalidCredentialsException);
                    }
                }

                var user = new Identity(session.IdentityId);
                var stream = _eventStore.Get(session.IdentityId);

                stream.Apply(user);
                stream.AddEvent(user.SetPassword(passwordHash));

                _eventStore.Save(stream);
            }

            return Accepted();
        }

        [HttpPost("resetpassword")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult ResetPassword([FromBody] ResetPassword message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var sessionTokenResult = HttpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return BadRequest(Access.Resources.SessionTokenException);
            }

            var passwordHash = _hashingService.Sha256(message.Password);

            using (_databaseContextFactory.Create())
            {
                var queryIdentity = _identityQuery.Search(new DataAccess.Query.Identity.Specification().WithName(message.Name)).SingleOrDefault();

                if (queryIdentity == null)
                {
                    return BadRequest();
                }

                var identity = new Identity(queryIdentity.Id);
                var stream = _eventStore.Get(queryIdentity.Id);

                stream.Apply(identity);

                if (identity.HasPasswordResetToken && identity.PasswordResetToken == message.PasswordResetToken)
                {
                    stream.AddEvent(identity.SetPassword(passwordHash));

                    _eventStore.Save(stream);
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        [HttpPost("rolestatus")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult RoleStatus([FromBody] GetIdentityRoleStatus message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<Guid> roles;

            using (_databaseContextFactory.Create())
            {
                roles = _identityQuery.RoleIds(new DataAccess.Query.Identity.Specification().WithIdentityId(message.IdentityId))
                    .ToList();
            }

            return Ok(
                from roleId in message.RoleIds
                select new IdentityRoleStatus
                {
                    RoleId = roleId,
                    Active = roles.Any(item => item.Equals(roleId))
                }
            );
        }

        [HttpPost("activate")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult Activate([FromBody] ActivateIdentity message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var specification = new DataAccess.Query.Identity.Specification();

            if (message.Id.HasValue)
            {
                specification.WithIdentityId(message.Id.Value);
            }
            else
            {
                specification.WithName(message.Name);
            }
            
            using (_databaseContextFactory.Create())
            {
                var query = _identityQuery.Search(specification).FirstOrDefault();

                if (query == null)
                {
                    return BadRequest();
                }

                _serviceBus.Send(message);
            }

            return Accepted();
        }

        [HttpPost("getpasswordresettoken")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult GetPasswordResetToken([FromBody] GetPasswordResetToken message)
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            using (_databaseContextFactory.Create())
            {
                var query = _identityQuery.Search(new DataAccess.Query.Identity.Specification().WithName(message.Name)).SingleOrDefault();

                if (query == null)
                {
                    return BadRequest();
                }

                var stream = _eventStore.Get(query.Id);
                var identity = new Identity(query.Id);
                
                stream.Apply(identity);

                if (identity.Activated)
                {
                    if (!identity.HasPasswordResetToken)
                    {
                        stream.AddEvent(identity.RegisterPasswordResetToken());

                        _eventStore.Save(stream);
                    }

                    return Ok(identity.PasswordResetToken);
                }
                else
                {
                    return BadRequest(Resources.IdentityInactiveException);
                }
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegisterIdentity message)
        {
            Guard.AgainstNull(message, nameof(message));

            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var registeredBy = "system";
            var result = HttpContext.GetAccessSessionToken();
            var ok = false;

            if (result.Ok)
            {
                using (_databaseContextFactory.Create())
                {
                    var session = _sessionRepository.Find(result.SessionToken);

                    if (session != null)
                    {
                        registeredBy = session.IdentityName;

                        ok = session.HasPermission(Permissions.Register.Identity);
                    }
                }
            }
            else
            {
                using (_databaseContextFactory.Create())
                {
                    if (_identityQuery.Count(new DataAccess.Query.Identity.Specification()) == 0 &&
                        _authorizationService is IAnonymousPermissions anonymousPermissions)
                    {
                        ok = anonymousPermissions.AnonymousPermissions()
                            .Any(item => item.Equals(Permissions.Register.Identity));
                    }
                }
            }

            if (!ok)
            {
                return Unauthorized();
            }

            var generatedPassword = string.Empty;

            if (string.IsNullOrWhiteSpace(message.Password))
            {
                generatedPassword = _passwordGenerator.Generate();
                message.Password = generatedPassword;
            }

            var sessionTokenResult = HttpContext.GetAccessSessionToken();

            _serviceBus.Send(new RegisterIdentity
            {
                Name = message.Name,
                PasswordHash = _hashingService.Sha256(message.Password),
                RegisteredBy = registeredBy,
                GeneratedPassword = generatedPassword,
                Activated = message.Activated && sessionTokenResult.Ok && _accessService.HasPermission(sessionTokenResult.SessionToken, Permissions.Activate.Identity),
                System = message.System
            });

            return Accepted();
        }
    }
}