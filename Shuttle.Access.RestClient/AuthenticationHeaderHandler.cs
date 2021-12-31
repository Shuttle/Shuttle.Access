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
        private readonly string _userAgent;

        public string Token { get; set; }

        public AuthenticationHeaderHandler()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            _userAgent = $"Shuttle.Access/{version.Major}.{version.Minor}.{version.Build}";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(request, nameof(request));

            request.Headers.Add("User-Agent", _userAgent);

            if (!string.IsNullOrEmpty(Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("access-sessiontoken", Token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}