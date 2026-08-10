using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public OptionsBuilder<AccessAuthorizationOptions> Options => Services.AddOptions<AccessAuthorizationOptions>();

    /// <summary>
    ///     Replaces the default <see cref="DelegatedSessionResolver" />, which asks the Shuttle.Access web API to
    ///     establish the session.  This is intended for the Shuttle.Access web API itself, which is the authority and
    ///     therefore has to resolve the session from the credential directly.
    /// </summary>
    public AccessAuthorizationBuilder UseSessionResolver<T>() where T : class, ISessionResolver
    {
        Services.Replace(ServiceDescriptor.Scoped<ISessionResolver, T>());

        // `BaseAddress` is only required by the `DelegatedSessionResolver` being replaced here, so its validator no
        // longer applies — an application that resolves sessions itself has no web API to call.
        var validator = Services.FirstOrDefault(item =>
            item.ServiceType == typeof(IValidateOptions<AccessAuthorizationOptions>) &&
            item.ImplementationType == typeof(AccessAuthorizationOptionsValidator));

        if (validator != null)
        {
            Services.Remove(validator);
        }

        return this;
    }
}
