using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi.v1
{
    [Route("[controller]", Order = 1)]
    [Route("v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1")]
    public class IdentitiesController : Controller
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IIdentityQuery _identityQuery;
        private readonly IMediator _mediator;
        private readonly IServiceBus _serviceBus;

        public IdentitiesController(IDatabaseContextFactory databaseContextFactory, IServiceBus serviceBus,
            IIdentityQuery identityQuery, IMediator mediator)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(mediator, nameof(mediator));

            _databaseContextFactory = databaseContextFactory;
            _serviceBus = serviceBus;
            _identityQuery = identityQuery;
            _mediator = mediator;
        }

        [HttpPatch("{id}/name")]
        [RequiresPermission(Permissions.Register.Role)]
        public IActionResult SetName(Guid id, [FromBody] SetIdentityName message)
        {
            try
            {
                message.ApplyInvariants();
                message.Id = id;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _serviceBus.Send(message);

            return Accepted();
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
            _serviceBus.Send(new RemoveIdentity
            {
                Id = id
            });

            return Accepted();
        }

        [HttpPatch("{id}/roles/{roleId}")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult SetRole(Guid id, Guid roleId, [FromBody] SetIdentityRole message)
        {
            try
            {
                message.ApplyInvariants();
                message.IdentityId = id;
                message.RoleId = roleId;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            using (_databaseContextFactory.Create())
            {
                var reviewRequest = new RequestMessage<SetIdentityRole>(message);

                _mediator.Send(reviewRequest);

                if (!reviewRequest.Ok)
                {
                    return BadRequest(reviewRequest.Message);
                }
            }

            _serviceBus.Send(message);

            return Accepted();
        }

        [HttpPut("password/change")]
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

            message.Token = sessionTokenResult.SessionToken;

            using (_databaseContextFactory.Create())
            {
                var changePassword = new RequestMessage<ChangePassword>(message);

                _mediator.Send(changePassword);

                if (!changePassword.Ok)
                {
                    return BadRequest(changePassword.Message);
                }
            }

            return Accepted();
        }

        [HttpPut("password/reset")]
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

            var requestMessage = new RequestMessage<ResetPassword>(message);

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(requestMessage);
            }

            return !requestMessage.Ok ? BadRequest(requestMessage.Message) : Ok();
        }

        [HttpPost("{id}/roles/availability")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult RolesSearch(Guid id, [FromBody] Identifiers<Guid> identifiers)
        {
            try
            {
                identifiers.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<Guid> roles;

            using (_databaseContextFactory.Create())
            {
                roles = _identityQuery.RoleIds(new DataAccess.Query.Identity.Specification().WithIdentityId(id))
                    .ToList();

                return Ok(from roleId in identifiers.Values
                    select new IdentifierAvailability<Guid>()
                    {
                        Id = roleId,
                        Active = roles.Any(item => item.Equals(roleId))
                    });
            }
        }

        [HttpPut("activate")]
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

        [HttpGet("{name}/password/reset-token")]
        [RequiresPermission(Permissions.Register.Identity)]
        public IActionResult GetPasswordResetToken(string name)
        {
            var message = new GetPasswordResetToken
            {
                Name = name
            };

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
                var requestResponse = new RequestResponseMessage<GetPasswordResetToken, Guid>(message);

                _mediator.Send(requestResponse);

                return !requestResponse.Ok ? BadRequest(requestResponse.Message) : Ok(requestResponse.Response);
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

            var sessionTokenResult = HttpContext.GetAccessSessionToken();
            var identityRegistrationRequested =
                new IdentityRegistrationRequested(sessionTokenResult.Ok ? sessionTokenResult.SessionToken : null);

            using (_databaseContextFactory.Create())
            {
                _mediator.Send(identityRegistrationRequested);
            }

            if (!identityRegistrationRequested.IsAllowed)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(message.Password))
            {
                var generatePassword = new GeneratePassword();

                _mediator.Send(generatePassword);

                message.Password = generatePassword.GeneratedPassword;
            }

            var generateHash = new GenerateHash { Value = message.Password };

            _mediator.Send(generateHash);

            message.Password = string.Empty;
            message.PasswordHash = generateHash.Hash;
            message.RegisteredBy = identityRegistrationRequested.RegisteredBy;
            message.Activated = message.Activated && sessionTokenResult.Ok &&
                                identityRegistrationRequested.IsActivationAllowed;
            message.System = message.System;

            _serviceBus.Send(message);

            return Accepted();
        }
    }
}