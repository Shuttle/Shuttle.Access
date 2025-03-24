using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AccessHttpMessageHandler : DelegatingHandler
{
    private readonly AccessClientOptions _accessClientOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _userAgent;

    public AccessHttpMessageHandler(IOptions<AccessClientOptions> accessClientOptions, IServiceProvider serviceProvider)
    {
        _accessClientOptions = Guard.AgainstNull(accessClientOptions).Value;
        _serviceProvider = Guard.AgainstNull(serviceProvider);

        var version = Assembly.GetExecutingAssembly().GetName().Version;

        _userAgent = $"Shuttle.Access{(version != null ? $"/{version.Major}.{version.Minor}.{version.Build}" : string.Empty)}";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(request);

        request.Headers.Add("User-Agent", _userAgent);

        await (_accessClientOptions.ConfigureHttpRequestAsync?.Invoke(request, _serviceProvider) ?? Task.CompletedTask);

        return await base.SendAsync(request, cancellationToken);
    }
}