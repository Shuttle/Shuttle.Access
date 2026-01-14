using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Session), Schema = "access")]
[Index(nameof(Token), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(Token)}")]
[Index(nameof(IdentityId), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityId)}")]
[Index(nameof(IdentityName), IsUnique = true, Name = $"UX_{nameof(Session)}_{nameof(IdentityName)}")]
public class Session
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTimeOffset RegisteredAt { get; set; }

    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    public Identity Identity { get; set; } = null!;

    [Required]
    public Guid IdentityId { get; set; }

    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    [StringLength(320)]
    public string IdentityName { get; set; } = string.Empty;

    public ICollection<SessionPermission> SessionPermissions { get; set; } = [];

    [Required]
    public byte[] Token { get; set; } = new byte[32];

    public class Specification
    {
        private readonly List<string> _permissions = [];

        public Guid? IdentityId { get; private set; }
        public string? IdentityName { get; private set; }
        public string? IdentityNameMatch { get; private set; }
        public int MaximumRows { get; private set; }
        public IEnumerable<string> Permissions => _permissions.AsReadOnly();

        public bool ShouldIncludePermissions { get; private set; }

        public byte[]? Token { get; private set; }

        public Specification AddPermission(string permission)
        {
            Guard.AgainstEmpty(permission);

            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
            }

            return this;
        }

        public Specification AddPermissions(IEnumerable<string> permissions)
        {
            Guard.AgainstNull(permissions);

            foreach (var permission in permissions)
            {
                AddPermission(permission);
            }

            return this;
        }

        public Specification IncludePermissions()
        {
            ShouldIncludePermissions = true;
            return this;
        }

        public Specification WithIdentityId(Guid identityId)
        {
            IdentityId = identityId;
            MaximumRows = 1;
            return this;
        }

        public Specification WithIdentityName(string identityName)
        {
            IdentityName = Guard.AgainstEmpty(identityName);
            MaximumRows = 1;

            return this;
        }

        public Specification WithIdentityNameMatch(string identityNameMatch)
        {
            IdentityNameMatch = Guard.AgainstEmpty(identityNameMatch);

            return this;
        }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }

        public Specification WithToken(byte[] token)
        {
            Token = Guard.AgainstNull(token);
            MaximumRows = 1;

            return this;
        }
    }
}