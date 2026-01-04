namespace Shuttle.Access.AspNetCore;

public class AccessPermissionRequirement(string permission)
{
    public string Permission { get; } = permission ?? throw new ArgumentNullException(nameof(permission));
}