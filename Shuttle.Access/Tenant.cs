using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Contract;

namespace Shuttle.Access;

public enum TenantStatus
{
    Active = 1,
    Disabled = 2
}

public class Tenant
{
    public string Name { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }

    public static string Key(string name)
    {
        return $"[tenant]:name={name}";
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        Name = registered.Name;

        return registered;
    }

    private StatusSet On(StatusSet statusSet)
    {
        Guard.AgainstNull(statusSet);

        Status = statusSet.Status;

        return statusSet;
    }

    private Removed On(Removed removed)
    {
        Guard.AgainstNull(removed);

        return removed;
    }

    public Registered Register(string name, int status)
    {
        return On(new Registered
        {
            Name = Guard.AgainstEmpty(name),
            Status = status
        });
    }

    public Removed Remove()
    {
        return On(new Removed());
    }

    public StatusSet SetStatus(TenantStatus status)
    {
        if (Status == status)
        {
            throw new InvalidOperationException(string.Format(Resources.ValueAlreadySetException, nameof(status), status));
        }

        return On(new StatusSet
        {
            Status = status
        });
    }
}