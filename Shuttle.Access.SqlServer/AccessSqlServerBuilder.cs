using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Access.SqlServer;

public class AccessSqlServerBuilder(IServiceCollection services)
{
    public AccessSqlServerOptions Options
    {
        get;
        set => field = Guard.AgainstNull(value);
    } = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}