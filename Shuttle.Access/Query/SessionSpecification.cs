using Shuttle.Access.SqlServer.Models;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Query;

public class SessionSpecification : Specification<SessionSpecification>
{
    private readonly List<string> _permissions = [];
    public Guid? IdentityId { get; private set; }
    public string? IdentityName { get; private set; }
    public string? IdentityNameMatch { get; private set; }
    public IEnumerable<string> Permissions => _permissions.AsReadOnly();

    public bool ShouldIncludePermissions { get; private set; }

    public bool ShouldIncludeTenantId { get; private set; }

    public Guid? TenantId { get; private set; }

    public byte[]? Token { get; private set; }

    public SessionSpecification AddPermission(string permission)
    {
        Guard.AgainstEmpty(permission);

        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }

        return this;
    }

    public SessionSpecification AddPermissions(IEnumerable<string> permissions)
    {
        Guard.AgainstNull(permissions);

        foreach (var permission in permissions)
        {
            AddPermission(permission);
        }

        return this;
    }

    public SessionSpecification IncludePermissions()
    {
        ShouldIncludePermissions = true;
        return this;
    }

    public SessionSpecification WithIdentityId(Guid identityId)
    {
        IdentityId = identityId;
        return this;
    }

    public SessionSpecification WithIdentityName(string identityName)
    {
        IdentityName = Guard.AgainstEmpty(identityName);
        return this;
    }

    public SessionSpecification WithIdentityNameMatch(string identityNameMatch)
    {
        IdentityNameMatch = Guard.AgainstEmpty(identityNameMatch);

        return this;
    }

    public SessionSpecification WithTenantId(Guid? tenantId)
    {
        TenantId = tenantId;
        ShouldIncludeTenantId = true;
        return this;
    }

    public SessionSpecification WithToken(byte[] token)
    {
        Token = Guard.AgainstNull(token);
        WithMaximumRows(1);
        return this;
    }
}