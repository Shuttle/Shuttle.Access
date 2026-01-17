using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class Tenant
{
    public string Name { get; private set; } = string.Empty;
    public string LogoSvg { get; private set; } = string.Empty;
    public string LogoUrl { get; private set; } = string.Empty;
    public int Status { get; private set; }

    public Registered Register(string name, int status, string logoSvg, string logoUrl)
    {
        return On(new()
        {
            Name = Guard.AgainstEmpty(name),
            LogoSvg = logoSvg,
            LogoUrl = logoUrl,
            Status = status
        });
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        Name = registered.Name;
        LogoSvg = registered.LogoSvg;
        LogoUrl = registered.LogoUrl;

        return registered;
    }

    public static string Key(string name)
    {
        return $"[tenant]:name={name}";
    }
}