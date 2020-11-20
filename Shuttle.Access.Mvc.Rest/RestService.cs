using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RestSharp;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.Rest
{
    public class RestService : IRestService
    {
        private readonly RestClient _client;
        private readonly string _url;

        public RestService(IRestConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _url = configuration.Url;

            if (!_url.EndsWith("/"))
            {
                _url += "/";
            }

            _client = new RestClient(_url);
        }

        public IEnumerable<string> GetPermissions(Guid token)
        {
            var request = new RestRequest(GetUrl($"sessions/{token}"), Method.GET, DataFormat.Json);

            try
            {
                var response = _client.Get<List<string>>(request);

                return response.StatusCode == HttpStatusCode.OK ? response.Data : Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public string GetUrl(string path)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            return path.StartsWith("/")
                ? $"{_url}{path.Substring(1)}"
                : $"{_url}{path}";
        }
    }
}