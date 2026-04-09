using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Access;

public class AccessBuilder(IServiceCollection services)
{
    public AccessOptions Options
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}