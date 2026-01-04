using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ReviewSetIdentityRoleParticipant(IRoleQuery roleQuery, IIdentityQuery identityQuery) : IParticipant<RequestMessage<SetIdentityRole>>
{
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task ProcessMessageAsync(RequestMessage<SetIdentityRole> message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).Request;
        var roles = (await _roleQuery.SearchAsync(new SqlServer.Models.Role.Specification().AddName("Access Administrator"), cancellationToken)).ToList();

        if (roles.Count != 1)
        {
            return;
        }

        var role = roles[0];

        if (request.RoleId.Equals(role.Id) &&
            !request.Active &&
            await _identityQuery.AdministratorCountAsync(cancellationToken) == 1)
        {
            message.Failed("last-administrator");
        }
    }
}