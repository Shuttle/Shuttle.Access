using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly IComponentResolver _resolver;
        private readonly string _userAgent;

        public AuthenticationHeaderHandler(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            _resolver = resolver;

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            _userAgent = $"Shuttle.Access/{version.Major}.{version.Minor}.{version.Build}";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));

            request.Headers.Add("User-Agent", _userAgent);

            var client = _resolver.Resolve<IAccessClient>();

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