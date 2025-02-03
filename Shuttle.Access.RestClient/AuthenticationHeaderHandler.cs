using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly AccessClientOptions _accessClientOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _userAgent;

    public AuthenticationHeaderHandler(IOptions<AccessClientOptions> accessClientOptions, IServiceProvider serviceProvider)
    {
        // Cannot inject IAccessClient directly as it relies on this handler, which seems to result in a circular dependency:
        // InvalidOperationException: ValueFactory attempted to access the Value property of this instance.

        _accessClientOptions = Guard.AgainstNull(accessClientOptions).Value;
        _serviceProvider = Guard.AgainstNull(serviceProvider);

        var version = Assembly.GetExecutingAssembly().GetName().Version;

        _userAgent = $"Shuttle.Access{(version != null ? $"/{version.Major}.{version.Minor}.{version.Build}" : string.Empty)}";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(request);

        request.Headers.Add("User-Agent", _userAgent);

        var client = _serviceProvider.GetRequiredService<IAccessClient>();

        if ((!client.Token.HasValue || (client.TokenExpiryDate ?? DateTime.UtcNow).Subtract(_accessClientOptions.RenewToleranceTimeSpan) < DateTime.UtcNow) &&
            !(request.RequestUri?.PathAndQuery ?? string.Empty).Equals("/sessions") && request.Method != HttpMethod.Post)
        {
            await client.RegisterSessionAsync(cancellationToken);
        }

        if (client.Token.HasValue)
        {
            request.Headers.Authorization = new("Shuttle.Access", $"token={client.Token.Value:D}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}