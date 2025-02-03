namespace Shuttle.Access.AspNetCore;

public class AccessPermissionRequirement
{
    public string Permission { get; }

    public AccessPermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}