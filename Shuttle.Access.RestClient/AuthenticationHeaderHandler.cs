using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AuthenticationHeaderHandler : HttpClientHandler
    {
        private readonly IAccessClient _client;
        private readonly string _userAgent;

        public AuthenticationHeaderHandler(IAccessClient client)
        {
            Guard.AgainstNull(client, nameof(client));

            _client = client;

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            _userAgent = $"Shuttle.Access/{version.Major}.{version.Minor}.{version.Build}";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));

            request.Headers.Add("User-Agent", _userAgent);

            if ((!_client.Token.HasValue || (_client.TokenExpiryDate ?? DateTime.UtcNow) < DateTime.UtcNow) &&
                !request.RequestUri.PathAndQuery.Equals("/sessions") &&
                request.Method != HttpMethod.Post)
            {
                _client.RegisterSession();
            }

            if (_client.Token.HasValue)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("access-session-token", _client.Token.Value.ToString("n"));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}