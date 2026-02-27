using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class ReviewIdentityRoleRemovalParticipant(IRoleQuery roleQuery, IIdentityQuery identityQuery) : IParticipant<ReviewIdentityRoleRemoval>
{
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);

    public async Task HandleAsync(ReviewIdentityRoleRemoval message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var roles = (await _roleQuery.SearchAsync(new RoleSpecification()
            .WithTenantId(message.TenantId)
            .AddName("Access Administrator"), cancellationToken)).ToList();

        if (roles.Count != 1)
        {
            return;
        }

        var role = roles[0];

        if (message.RoleId.Equals(role.Id) && 
            await _identityQuery.AdministratorCountAsync(message.TenantId, cancellationToken) == 1)
        {
            message.LastAdministrator();
        }
    }
}