using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi
{
    public class UsersController : AccessApiController
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
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(bus, "bus");
            Guard.AgainstNull(hashingService, "hashingService");
            Guard.AgainstNull(sessionRepository, "sessionRepository");
            Guard.AgainstNull(authorizationService, "authorizationService");
            Guard.AgainstNull(systemUserQuery, "systemUserQuery");

            _databaseContextFactory = databaseContextFactory;
            _bus = bus;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _authorizationService = authorizationService;
            _systemUserQuery = systemUserQuery;
        }

        [RequiresPermission(SystemPermissions.Manage.Users)]
        public IHttpActionResult Get()
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
        public IHttpActionResult Get(Guid id)
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
        [Route("api/users/{id}/roles")]
        public IHttpActionResult GetRoles(Guid id)
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
        [Route("api/users/{id}")]
        public IHttpActionResult Delete(Guid id)
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
        [Route("api/users/setrole")]
        public IHttpActionResult SetRole([FromBody] SetUserRoleModel model)
        {
            Guard.AgainstNull(model, "model");

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
        [Route("api/users/rolestatus")]
        public IHttpActionResult RoleStatus([FromBody] UserRoleStatusModel model)
        {
            Guard.AgainstNull(model, "model");

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

        public IHttpActionResult Post([FromBody] RegisterUserModel model)
        {
            Guard.AgainstNull(model, "model");

            var registeredBy = "system";
            var result = GetSessionToken();
            var ok = false;

            if (result.OK)
            {
                using (_databaseContextFactory.Create())
                {
                    var session = _sessionRepository.Get(result.SessionToken);

                    registeredBy = session.Username;

                    ok = session.HasPermission(SystemPermissions.Manage.Users);
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
                    var anonymousPermissions = _authorizationService as IAnonymousPermissions;

                    if (anonymousPermissions != null)
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