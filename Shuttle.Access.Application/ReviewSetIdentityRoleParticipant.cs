using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ReviewSetIdentityRoleParticipant(IOptions<AccessOptions> accessOptions, IRoleQuery roleQuery, IIdentityQuery identityQuery) : IParticipant<RequestMessage<SetIdentityRoleStatus>>
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task HandleAsync(RequestMessage<SetIdentityRoleStatus> context, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(context).Request;
        var roles = (await _roleQuery.SearchAsync(new RoleSpecification()
            .WithTenantId(_accessOptions.SystemTenantId)
            .AddName("Access Administrator"), cancellationToken)).ToList();

        if (roles.Count != 1)
        {
            return;
        }

        var role = roles[0];

        if (request.RoleId.Equals(role.Id) &&
            !request.Active &&
            await _identityQuery.AdministratorCountAsync(cancellationToken) == 1)
        {
            context.Failed("last-administrator");
        }
    }
}