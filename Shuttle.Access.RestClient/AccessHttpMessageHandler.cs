using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class AccessHttpMessageHandler : DelegatingHandler
{
    private readonly AccessClientOptions _accessClientOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _userAgent;

    public AccessHttpMessageHandler(IOptions<AccessClientOptions> accessClientOptions, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _accessClientOptions = Guard.AgainstNull(Guard.AgainstNull(accessClientOptions).Value);
        _httpContextAccessor = Guard.AgainstNull(httpContextAccessor);
        _serviceProvider = Guard.AgainstNull(serviceProvider);

        var version = Assembly.GetExecutingAssembly().GetName().Version;

        _userAgent = $"Shuttle.Access{(version != null ? $"/{version.Major}.{version.Minor}.{version.Build}" : string.Empty)}";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(request);

        request.Headers.Add("User-Agent", _userAgent);

        if (_accessClientOptions.ConfigureHttpRequestAsync != null)
        {
            await _accessClientOptions.ConfigureHttpRequestAsync.Invoke(request, _serviceProvider);
        }
        else
        {
            var httpRequest = _httpContextAccessor.HttpContext?.Request;

            if ((httpRequest?.Headers.TryGetValue("Authorization", out var authorizationValues) ?? false) &&
                AuthenticationHeaderValue.TryParse(authorizationValues.ToString(), out var authenticationHeaderValue))
            {
                request.Headers.Authorization = authenticationHeaderValue;
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}