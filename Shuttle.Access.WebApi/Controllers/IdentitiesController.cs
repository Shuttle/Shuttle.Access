using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall;

namespace Shuttle.Access.WebApi
{
    [Route("[controller]")]
    public class IdentitiesController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly ISessionRepository _sessionRepository;
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;

        public IdentitiesController(IDatabaseContextFactory databaseContextFactory, IServiceBus bus,
            IHashingService hashingService, ISessionRepository sessionRepository,
            IAuthenticationService authenticationService, IAuthorizationService authorizationService,
            IIdentityQuery identityQuery, IRoleQuery roleQuery, IEventStore eventStore,
            IPasswordGenerator passwordGenerator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(passwordGenerator, nameof(passwordGenerator));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _identityQuery = identityQuery;
            _roleQuery = roleQuery;
            _eventStore = eventStore;
            _passwordGenerator = passwordGenerator;
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

        
        [HttpGet("{id}")]
        [RequiresPermission(Permissions.View.Identity)]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var user = _identityQuery
                    .Search(new DataAccess.Query.Identity.Specification().WithIdentityId(id).IncludeRoles()).FirstOrDefault();

                return user != null
                    ? (IActionResult) Ok(user)
                    : BadRequest();
            }
        }

        [HttpGet("{name}")]
        [RequiresPermission(Permissions.View.Identity)]
        public IActionResult Get(string name)
        {
            using (_databaseContextFactory.Create())
            {
                var user = _identityQuery
                    .Search(new DataAccess.Query.Identity.Specification().WithName(name).IncludeRoles()).FirstOrDefault();

                return user != null
                    ? (IActionResult) Ok(user)
                    : BadRequest();
            }
        }

        [HttpDelete("{id}")]
        [RequiresPermission(Permissions.Remove.Identity)]
        public IActionResult Delete(Guid id)
        {
            _bus.Send(new RemoveIdentityCommand
            {
                Id = id
            });

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("setrole")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult SetRole([FromBody] SetIdentityRoleModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                var roles = _roleQuery.Search(
                    new DataAccess.Query.Role.Specification().WithRoleName("Administrator")).ToList();

                if (roles.Count == 1)
                {
                    var role = roles[0];

                    if (model.RoleId.Equals(role.Id)
                        &&
                        !model.Active
                        &&
                        _identityQuery.AdministratorCount() == 1)
                    {
                        return Ok(new
                        {
                            Success = false,
                            FailureReason = "last-administrator"
                        });
                    }
                }
            }

            _bus.Send(new SetIdentityRoleCommand
            {
                IdentityId = model.IdentityId,
                RoleId = model.RoleId,
                Active = model.Active
            });

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("setpassword")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult SetPassword([FromBody] SetPasswordModel model)
        {
            try
            {
                model.ApplyInvariants();
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

            var passwordHash = _hashingService.Sha256(model.NewPassword);

            using (_databaseContextFactory.Create())
            {
                var session = _sessionRepository.Find(sessionTokenResult.SessionToken);

                if (session == null)
                {
                    return BadRequest(Access.Resources.SessionTokenException);
                }

                if (!string.IsNullOrWhiteSpace(model.OldPassword) && string.IsNullOrWhiteSpace(model.Token))
                {
                    var authenticationResult = _authenticationService.Authenticate(session.IdentityName, model.OldPassword);

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

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("rolestatus")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult RoleStatus([FromBody] IdentityRoleStatusModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<Guid> roles;

            using (_databaseContextFactory.Create())
            {
                roles = _identityQuery.RoleIds(new DataAccess.Query.Identity.Specification().WithIdentityId(model.IdentityId))
                    .ToList();
            }

            return Ok(
                from roleId in model.RoleIds
                select new
                {
                    RoleId = roleId,
                    Active = roles.Any(item => item.Equals(roleId))
                }
            );
        }

        [HttpPost("activate")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult Activate([FromBody] IdentityActivateModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var specification = new DataAccess.Query.Identity.Specification();

            if (model.Id.HasValue)
            {
                specification.WithIdentityId(model.Id.Value);
            }
            else
            {
                specification.WithName(model.Name);
            }
            
            using (_databaseContextFactory.Create())
            {
                var query = _identityQuery.Search(specification).FirstOrDefault();

                if (query == null)
                {
                    return BadRequest();
                }

                var stream = _eventStore.Get(query.Id);
                var identity = new Identity(query.Id);
                
                stream.Apply(identity);

                if (!identity.Activated)
                {
                    stream.AddEvent(identity.Activate(model.DateActivated));

                    _eventStore.Save(stream);
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegisterIdentityModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            try
            {
                model.ApplyInvariants();
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

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                generatedPassword = _passwordGenerator.Generate();
                model.Password = generatedPassword;
            }

            _bus.Send(new RegisterIdentityCommand
            {
                Name = model.Name,
                PasswordHash = _hashingService.Sha256(model.Password),
                RegisteredBy = registeredBy,
                GeneratedPassword = generatedPassword,
                Activated = model.Activated
            });

            return Ok();
        }
    }
}