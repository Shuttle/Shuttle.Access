using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Access.Sql;
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
        private readonly ISystemUserQuery _systemUserQuery;

        public UsersController(IDatabaseContextFactory databaseContextFactory, IServiceBus bus,
            IHashingService hashingService, ISessionRepository sessionRepository,
            IAuthorizationService authorizationService, ISystemUserQuery systemUserQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authorizationService = authorizationService;
            _systemUserQuery = systemUserQuery;
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpGet]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = from row in _systemUserQuery.Search()
                    select new
                    {
                        Id = SystemUserColumns.Id.MapFrom(row),
                        Username = SystemUserColumns.Username.MapFrom(row),
                        DateRegistered = SystemUserColumns.DateRegistered.MapFrom(row),
                        RegisteredBy = SystemUserColumns.RegisteredBy.MapFrom(row)
                    }
                });
            }
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _systemUserQuery.Get(id)
                });
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
            Guard.AgainstNull(model, nameof(model));

            using (_databaseContextFactory.Create())
            {
                if (model.RoleName.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase)
                    &&
                    !model.Active
                    &&
                    _systemUserQuery.AdministratorCount() == 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        FailureReason = "LastAdministrator"
                    });
                }
            }

            _bus.Send(new SetUserRoleCommand
            {
                UserId = model.UserId,
                RoleName = model.RoleName,
                Active = model.Active
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
            Guard.AgainstNull(model, nameof(model));

            List<string> roles;

            using (_databaseContextFactory.Create())
            {
                roles = _systemUserQuery.Roles(model.UserId).ToList();
            }

            return Ok(
                new
                {
                    Data = from role in model.Roles
                    select new
                    {
                        RoleName = role,
                        Active = roles.Find(item => item.Equals(role)) != null
                    }
                });
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegisterUserModel model)
        {
            Guard.AgainstNull(model, nameof(model));

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
                    count = _systemUserQuery.Count();
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
                PasswordHash = _hashingService.Sha256(model.Password),
                RegisteredBy = registeredBy
            });

            return Ok();
        }
    }
}