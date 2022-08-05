using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _userAgent;

        public AuthenticationHeaderHandler(IServiceProvider serviceProvider)
        {
            Guard.AgainstNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            _userAgent = $"Shuttle.Access/{version.Major}.{version.Minor}.{version.Build}";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));

            request.Headers.Add("User-Agent", _userAgent);

            var client = _serviceProvider.GetRequiredService<IAccessClient>();

            if ((!client.Token.HasValue || (client.TokenExpiryDate ?? DateTime.UtcNow) < DateTime.UtcNow) &&
                !request.RequestUri.PathAndQuery.Equals("/sessions") &&
                request.Method != HttpMethod.Post)
            {
                client.RegisterSession();
            }

            if (client.Token.HasValue)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("access-session-token", client.Token.Value.ToString("n"));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}