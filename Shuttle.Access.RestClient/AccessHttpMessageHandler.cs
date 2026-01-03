using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AccessHttpMessageHandler : DelegatingHandler
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions;
    private readonly AccessClientOptions _accessClientOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _userAgent;

    public AccessHttpMessageHandler(IOptions<AccessClientOptions> accessClientOptions, IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
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

        if (_accessAuthorizationOptions.PassThrough)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null &&
                httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues) &&
                AuthenticationHeaderValue.TryParse(authorizationHeaderValues, out var authorizationHeader))
            {
                request.Headers.Authorization = authorizationHeader;
            }
        }

        await (_accessClientOptions.ConfigureHttpRequestAsync?.Invoke(request, _serviceProvider) ?? Task.CompletedTask);

        return await base.SendAsync(request, cancellationToken);
    }
}