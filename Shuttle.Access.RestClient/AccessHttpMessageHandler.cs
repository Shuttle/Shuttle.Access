using System.Reflection;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

/// <summary>
///     Applies this application's own credential to every outgoing request.  An <see cref="IAuthenticationInterceptor" />
///     is always registered — the REST client exists so that an application can call the Shuttle.Access web API as
///     itself, which is only meaningful when it has an identity.
/// </summary>
public class AccessHttpMessageHandler(IAuthenticationInterceptor authenticationInterceptor) : DelegatingHandler
{
    private static readonly string UserAgent = $"Shuttle.Access{(Assembly.GetExecutingAssembly().GetName().Version is { } version ? $"/{version.Major}.{version.Minor}.{version.Build}" : string.Empty)}";

    private readonly IAuthenticationInterceptor _authenticationInterceptor = Guard.AgainstNull(authenticationInterceptor);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(request).Headers.Add("User-Agent", UserAgent);

        await _authenticationInterceptor.ConfigureAsync(request, cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
