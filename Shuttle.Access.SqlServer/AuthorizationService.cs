using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class AuthorizationService(IIdentityQuery identityQuery) : IAuthorizationService
{
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task<IEnumerable<Messages.v1.Permission>> GetPermissionsAsync(string identityName, CancellationToken cancellationToken = default)
    {
        var result = new List<Messages.v1.Permission>();

        var permissions = await _identityQuery.PermissionsAsync(await _identityQuery.IdAsync(identityName, cancellationToken), cancellationToken);

        foreach (var permission in permissions)
        {
            result.Add(new()
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Description = permission.Description,
                    Status = permission.Status
                }
            );
        }

        return result;
    }
}