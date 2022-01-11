using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class ReviewSetIdentityRoleStatusParticipant : IParticipant<RequestMessage<SetIdentityRoleStatus>>
    {
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;

        public ReviewSetIdentityRoleStatusParticipant(IRoleQuery roleQuery, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _roleQuery = roleQuery;
            _identityQuery = identityQuery;
        }

        public void ProcessMessage(IParticipantContext<RequestMessage<SetIdentityRoleStatus>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var roles = _roleQuery.Search(
                new DataAccess.Query.Role.Specification().WithRoleName("Administrator")).ToList();

            if (roles.Count != 1)
            {
                return;
            }

            var role = roles[0];

            if (request.RoleId.Equals(role.Id)
                &&
                !request.Active
                &&
                _identityQuery.AdministratorCount() == 1)
            {
                context.Message.Failed("last-administrator");
            }
        }
    }
}