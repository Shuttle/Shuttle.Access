namespace Shuttle.Access.AspNetCore;

public class AccessPermissionRequirement
{
    public AccessPermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public string Permission { get; }
}