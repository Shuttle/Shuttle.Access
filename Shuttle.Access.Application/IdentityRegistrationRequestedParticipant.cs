using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class IdentityRegistrationRequestedParticipant : IParticipant<IdentityRegistrationRequested>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IIdentityQuery _identityQuery;

        public IdentityRegistrationRequestedParticipant(IAuthorizationService authorizationService, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(authorizationService, nameof(authorizationService));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _authorizationService = authorizationService;
            _identityQuery = identityQuery;
        }

        public void ProcessMessage(IParticipantContext<IdentityRegistrationRequested> context)
        {
            Guard.AgainstNull(context, nameof(context));

            if (context.Message.SessionToken.HasValue)
            {
                return;
            }

            if (_identityQuery.Count(new DataAccess.Query.Identity.Specification()) == 0 &&
                _authorizationService is IAnonymousPermissions anonymousPermissions &&
                anonymousPermissions.AnonymousPermissions().Any(item => item.Equals(Permissions.Register.Identity)))
            {
                context.Message.Allowed("system");
            }
        }
    }
}