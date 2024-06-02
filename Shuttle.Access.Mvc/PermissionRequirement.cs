using Microsoft.AspNetCore.Authorization;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = Guard.AgainstNullOrEmptyString(permission, nameof(permission));
    }
}