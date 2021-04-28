using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Api
{
    public class AccessClient : IAccessClient
    {
        private static readonly object Lock = new object();
        private readonly IRestClient _client;
        private readonly IClientConfiguration _configuration;
        private string _token;
        private DateTime _tokenExpiryDate;

        public AccessClient(IClientConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _client = new RestClient(configuration.Url);
        }

        public bool HasSession => !string.IsNullOrWhiteSpace(_token);

        public T Get<T>(RestRequest request) where T : new()
        {
            lock (Lock)
            {
                Safeguard(request);

                return (T) _client.Execute<T>(request);
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


        public void Register(string name, string password, string system)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var request = new RestRequest(_configuration.GetApiUrl("identities"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(new
            {
                name,
                password,
                system
            });

            var response = GetResponse(request);

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

                try
                {
                    var request = new RestRequest(_configuration.GetApiUrl("sessions"))
                    {
                        Method = Method.POST,
                        RequestFormat = DataFormat.Json
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

                    if (result == null || result.success == null)
                    {
                        throw new ApiException(string.Format(Resources.ResponseMissingAttributeException, "success"));
                    }

                    if (!Convert.ToBoolean(result.success.ToString()))
                    {
                        throw new ApiException(Resources.LoginException);
                    }

                    if (result.token == null || result.tokenExpiryDate == null)
                    {
                        throw new ApiException(string.Format(Resources.ResponseMissingAttributeException,
                            "token/tokenExpiryDate"));
                    }

                    _token = result.token;
                    _tokenExpiryDate = (DateTime) result.tokenExpiryDate;
                }
                catch
                {
                    throw new ApiException(Resources.LoginException);
                }
            }
        }

        public void Activate(string name, DateTime dateActivated)
        {
            Activate(new {name, dateActivated});
        }

        public void Activate(Guid id, DateTime dateActivated)
        {
            Activate(new {id, dateActivated});
        }

        public Guid GetPasswordResetToken(string name)
        {
            var request = new RestRequest(_configuration.GetApiUrl("identities/getpasswordresettoken"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(new {name});

            var response = GetResponse(request);

            if (!response.IsSuccessful)
            {
                throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            }

            var passwordResetToken = response.AsDynamic().passwordResetToken;

            return passwordResetToken == null ? Guid.Empty : (Guid) passwordResetToken;
        }

        public void ResetPassword(string name, Guid passwordResetToken, string password)
        {
            var request = new RestRequest(_configuration.GetApiUrl("identities/resetpassword"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(new {name, passwordResetToken, password});

            var response = GetResponse(request);

            if (!response.IsSuccessful)
            {
                throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            }
        }

        public RegisterSessionResult RegisterSession(string identityName)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            var request = new RestRequest(_configuration.GetApiUrl("sessions/request"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(new {identityName});

            var response = GetResponse(request);

            if (!response.IsSuccessful)
            {
                throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            }

            var content = response.AsDynamic();

            if (content.success != null && (bool) content.success)
            {
                var permissions = new List<string>();

                foreach (var permission in content.permissions)
                {
                    permissions.Add(permission.permission.ToString());
                }

                return RegisterSessionResult.Success(identityName, (Guid) content.token,
                    (DateTime) content.tokenExpiryDate, permissions);
            }

            return RegisterSessionResult.Failure();
        }

        private void Safeguard(RestRequest request)
        {
            Guard.AgainstNull(request, nameof(request));

            if (!HasSession || DateTime.Now > _tokenExpiryDate && !request.Resource.Contains("login"))
            {
                _token = string.Empty;
                Login();
            }

            if (request.Parameters.Find(item =>
                (item.Name ?? string.Empty).Equals("access-sessiontoken",
                    StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                request.AddHeader("access-sessiontoken", _token);
            }
        }

        private void ResetSession()
        {
            _token = string.Empty;
        }

        public void Activate(dynamic model)
        {
            var request = new RestRequest(_configuration.GetApiUrl("identities/activate"))
            {
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(model);

            var response = GetResponse(request);

            if (!response.IsSuccessful)
            {
                throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            }
        }
    }
}