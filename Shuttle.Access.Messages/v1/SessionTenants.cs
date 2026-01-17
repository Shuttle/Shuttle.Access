using System.ComponentModel.DataAnnotations;

namespace Shuttle.Access.Messages.v1;

public class SessionTenants
{
    public Guid SessionId { get; set; }
    public List<Tenant> Tenants { get; set; } = [];

    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoSvg { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }
}