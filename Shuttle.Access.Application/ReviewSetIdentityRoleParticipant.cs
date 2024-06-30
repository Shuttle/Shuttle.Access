using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class ReviewSetIdentityRoleParticipant : IAsyncParticipant<RequestMessage<SetIdentityRole>>
    {
        private readonly IRoleQuery _roleQuery;
        private readonly IIdentityQuery _identityQuery;

        public ReviewSetIdentityRoleParticipant(IRoleQuery roleQuery, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _roleQuery = roleQuery;
            _identityQuery = identityQuery;
        }

        public async Task ProcessMessageAsync(IParticipantContext<RequestMessage<SetIdentityRole>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var roles = (await _roleQuery.SearchAsync(new RoleSpecification().AddName("Administrator"))).ToList();

            if (roles.Count != 1)
            {
                return;
            }

            var role = roles[0];

            if (request.RoleId.Equals(role.Id)
                &&
                !request.Active
                &&
                await _identityQuery.AdministratorCountAsync() == 1)
            {
                context.Message.Failed("last-administrator");
            }
        }
    }
}