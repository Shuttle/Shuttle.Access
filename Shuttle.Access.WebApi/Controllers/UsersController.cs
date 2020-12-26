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
    public class UsersController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public UsersController(IDatabaseContextFactory databaseContextFactory, IServiceBus bus,
            IHashingService hashingService, ISessionRepository sessionRepository,
            IAuthenticationService authenticationService, IAuthorizationService authorizationService,
            ISystemUserQuery systemUserQuery, ISystemRoleQuery systemRoleQuery, IEventStore eventStore,
            IPasswordGenerator passwordGenerator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(passwordGenerator, nameof(passwordGenerator));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _systemUserQuery = systemUserQuery;
            _systemRoleQuery = systemRoleQuery;
            _eventStore = eventStore;
            _passwordGenerator = passwordGenerator;
        }

        [HttpGet]
        [RequiresPermission(SystemPermissions.View.Users)]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_systemUserQuery.Search(new DataAccess.Query.User.Specification()));
            }
        }

        
        [HttpGet("{id}")]
        [RequiresPermission(SystemPermissions.View.Users)]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var user = _systemUserQuery
                    .Search(new DataAccess.Query.User.Specification().WithUserId(id).IncludeRoles()).FirstOrDefault();

                return user != null
                    ? (IActionResult) Ok(user)
                    : BadRequest();
            }
        }

        [HttpDelete("{id}")]
        [RequiresPermission(SystemPermissions.Manage.Users)]
        public IActionResult Delete(Guid id)
        {
            _bus.Send(new RemoveUserCommand
            {
                Id = id
            });

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("setrole")]
        [RequiresPermission(SystemPermissions.Manage.Users)]
        public IActionResult SetRole([FromBody] SetUserRoleModel model)
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
                var roles = _systemRoleQuery.Search(
                    new DataAccess.Query.Role.Specification().WithRoleName("Administrator")).ToList();

                if (roles.Count == 1)
                {
                    var role = roles[0];

                    if (model.RoleId.Equals(role.Id)
                        &&
                        !model.Active
                        &&
                        _systemUserQuery.AdministratorCount() == 1)
                    {
                        return Ok(new
                        {
                            Success = false,
                            FailureReason = "last-administrator"
                        });
                    }
                }
            }

            _bus.Send(new SetUserRoleCommand
            {
                UserId = model.UserId,
                RoleId = model.RoleId,
                Active = model.Active
            });

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("setpassword")]
        [RequiresPermission(SystemPermissions.Manage.Users)]
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
                return BadRequest(Resources.SessionTokenException);
            }

            var passwordHash = _hashingService.Sha256(model.NewPassword);

            using (_databaseContextFactory.Create())
            {
                var session = _sessionRepository.Find(sessionTokenResult.SessionToken);

                if (session == null)
                {
                    return BadRequest(Resources.SessionTokenException);
                }

                if (!string.IsNullOrWhiteSpace(model.OldPassword) && string.IsNullOrWhiteSpace(model.Token))
                {
                    var authenticationResult = _authenticationService.Authenticate(session.Username, model.OldPassword);

                    if (!authenticationResult.Authenticated)
                    {
                        return BadRequest(Resources.InvalidCredentialsException);
                    }
                }

                var user = new User(session.UserId);
                var stream = _eventStore.Get(session.UserId);

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
        [RequiresPermission(SystemPermissions.Manage.Users)]
        public IActionResult RoleStatus([FromBody] UserRoleStatusModel model)
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
                roles = _systemUserQuery.RoleIds(new DataAccess.Query.User.Specification().WithUserId(model.UserId))
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

        [HttpPost]
        public IActionResult Post([FromBody] RegisterUserModel model)
        {
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
                        registeredBy = session.Username;

                        ok = session.HasPermission(SystemPermissions.Manage.Users);
                    }
                }
            }
            else
            {
                using (_databaseContextFactory.Create())
                {
                    if (_systemUserQuery.Count(new DataAccess.Query.User.Specification()) == 0 &&
                        _authorizationService is IAnonymousPermissions anonymousPermissions)
                    {
                        ok = anonymousPermissions.AnonymousPermissions()
                            .Any(item => item.Equals(SystemPermissions.Manage.Users));
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

            _bus.Send(new RegisterUserCommand
            {
                Username = model.Username,
                PasswordHash = _hashingService.Sha256(model.Password),
                RegisteredBy = registeredBy,
                GeneratedPassword = generatedPassword
            });

            return Ok();
        }
    }
}