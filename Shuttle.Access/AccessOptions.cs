using Shuttle.Extensions.Options;

namespace Shuttle.Access;

public class AccessOptions
{
    public const string SectionName = "Shuttle:Access";
    public Guid SystemTenantId { get; set; } = new("c3ee3908-716b-48df-abda-33b49e09be97");
    public string SystemTenantName { get; set; } = "System";

    public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan SessionRenewalTolerance { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan SessionTokenExchangeValidityTimeSpan { get; set; } = TimeSpan.FromMinutes(1);

    public AsyncEvent<AuthenticationEventArgs> Authenticated { get; set; } = new();
    public AsyncEvent<AuthenticationEventArgs> AuthenticationUnknownIdentity { get; set; } = new();
    public AsyncEvent<AuthenticationEventArgs> AuthenticationFailed { get; set; } = new();
}
