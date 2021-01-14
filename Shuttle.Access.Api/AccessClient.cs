using System;
using Newtonsoft.Json;
using RestSharp;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Api
{
    public class AccessClient : IAccessClient
    {
        private static readonly object Lock = new object();
        private readonly IClientConfiguration _configuration;
        private IRestClient _client;
        private string _token;

        public AccessClient(IClientConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            
            _configuration = configuration;
            _client = new RestClient(configuration.Url);
            
            Login();
        }

        public bool HasSession => !string.IsNullOrWhiteSpace(_token);

        public T Get<T>(RestRequest request) where T : new()
        {
            lock (Lock)
            {
                Safeguard(request);

                return (T)_client.Execute<T>(request);
            }
        }

        private void Safeguard(RestRequest request)
        {
            Guard.AgainstNull(request, nameof(request));

            if (!HasSession && !request.Resource.Contains("sessions"))
            {
                Login();
            }
        
            if (request.Parameters.Find(item =>
                (item.Name??string.Empty).Equals("access-sessiontoken", StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                request.AddHeader("access-sessiontoken", _token);
            }
        }

        public IRestResponse GetResponse(RestRequest request)
        {
            lock (Lock)
            {
                Safeguard(request);

                return _client.Execute(request);
            }
        }

        private void ResetSession()
        {
            _token = string.Empty;
        }


        public void Register(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            var request = new RestRequest(_configuration.GetApiUrl("sessions"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddParameter("name", _configuration.IdentityName, ParameterType.GetOrPost);
            request.AddParameter("password", _configuration.Password, ParameterType.GetOrPost);

            var response = _client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw new ApiException($"[{response.StatusDescription}] : {response.Content}");
            }
        }

        public void Logout()
        {
            lock (Lock)
            {
                if (!HasSession)
                {
                    return;
                }

                var request = new RestRequest(_configuration.GetApiUrl("sessions"), Method.DELETE, DataFormat.Json);

                var response = GetResponse(request).AsDynamic();

                if (response.success == null)
                {
                    throw new ApiException(string.Format(Resources.ResponseMissingAttributeException, "success"));
                }

                if (!Convert.ToBoolean(response.success.ToString()))
                {
                    throw new ApiException(Resources.LogoutException);
                }

                ResetSession();
            }
        }

        public void Login()
        {
            lock (Lock)
            {
                if (HasSession)
                {
                    return;
                }

                var request = new RestRequest(_configuration.GetApiUrl("sessions"))
                {
                    Method = Method.POST,
                    RequestFormat = DataFormat.Json,
                };

                request.AddHeader("content-type", "application/json");

                request.AddJsonBody(new
                {
                    _configuration.IdentityName,
                    _configuration.Password
                });

                var response = _client.Execute(request);

                if (!response.IsSuccessful)
                {
                    throw new ApiException($"[{response.StatusDescription}] : {response.Content}");
                }

                var result = JsonConvert.DeserializeObject<dynamic>(response.Content);

                if (result.success == null)
                {
                    throw new ApiException(string.Format(Resources.ResponseMissingAttributeException, "success"));
                }

                if (!Convert.ToBoolean(result.success.ToString()))
                {
                    throw new ApiException(Resources.LoginException);
                }

                if (result.token == null)
                {
                    throw new ApiException(string.Format(Resources.ResponseMissingAttributeException, "token"));
                }

                _token = result.token;
            }
        }
    }
}