namespace Shuttle.Access.AspNetCore;

public class AccessSystemPermissionRequirement(string permission)
{
    public string Permission { get; } = permission ?? throw new ArgumentNullException(nameof(permission));
}
