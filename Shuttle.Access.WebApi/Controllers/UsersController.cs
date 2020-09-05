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

namespace Shuttle.Access.WebApi
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceBus _bus;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IHashingService _hashingService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public UsersController(IDatabaseContextFactory databaseContextFactory, IServiceBus bus,IHashingService hashingService, ISessionRepository sessionRepository, IAuthenticationService authenticationService,IAuthorizationService authorizationService, ISystemUserQuery systemUserQuery,ISystemRoleQuery systemRoleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _systemUserQuery = systemUserQuery;
            _systemRoleQuery = systemRoleQuery;
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpGet]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(_systemUserQuery.Search(new DataAccess.Query.User.Specification()));
            }
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpGet("{id}")]
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

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpDelete("{id}")]
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

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpPost("setrole")]
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

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpPost("resetpassword")]
        public IActionResult RegisterPasswordReset([FromBody] RegisterPasswordResetModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(new RegisterPasswordResetCommand
            {
                UserId = model.UserId,
                Token = model.Token
            });

            return Ok(new
            {
                Success = true
            });
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpPost("passwordexpiry")]
        public IActionResult RegisterPasswordExpiry([FromBody] RegisterPasswordExpiryModel model)
        {
            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _bus.Send(new RegisterPasswordExpiryCommand
            {
                UserId = model.UserId,
                ExpiryDate = model.ExpiryDate,
                NeverExpires = model.NeverExpires
            });

            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("setpassword")]
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

            var token = HttpContext.GetAccessSessionToken();

            if (string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(model.Token))
            {
                return BadRequest(Resources.SessionTokenException);
            }

            var session = _sessionRepository.Find(new Guid(string.IsNullOrWhiteSpace(token) ? model.Token : token));

            if (session == null)
            {
                return BadRequest(Resources.SessionTokenException);
            }

            var authenticationResult = _authenticationService.Authenticate(session.Username, model.OldPassword);

            if (!authenticationResult.Authenticated)
            {
                return BadRequest(Resources.InvalidCredentialsException);
            }

            _bus.Send(new SetPasswordCommand
            {
                Username = session.Username,
                PasswordHash = _hashingService.Sha256(model.NewPassword)
            });

            return Ok(new
            {
                Success = true
            });
        }

        [RequiresPermission(SystemPermissions.Manage.Roles)]
        [HttpPost("rolestatus")]
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
                roles = _systemUserQuery.Roles(new DataAccess.Query.User.Specification().WithUserId(model.UserId))
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
            var result = Request.GetSessionToken();
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
                int count;

                using (_databaseContextFactory.Create())
                {
                    count = _systemUserQuery.Count(new DataAccess.Query.User.Specification());
                }

                if (count == 0)
                {
                    if (_authorizationService is IAnonymousPermissions anonymousPermissions)
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

            _bus.Send(new RegisterUserCommand
            {
                Username = model.Username,
                PasswordHash = !string.IsNullOrWhiteSpace(model.Password)
                    ? _hashingService.Sha256(model.Password)
                    : null,
                RegisteredBy = registeredBy
            });

            return Ok();
        }
    }
}