﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.RestClient.v1;
using Shuttle.Access.WebApi.Models.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AccessClient : IAccessClient
    {
        private static readonly object Lock = new object();
        private readonly IAccessClientConfiguration _configuration;
        private readonly AuthenticationHeaderHandler _handler;

        public AccessClient(IAccessClientConfiguration configuration, HttpClient httpClient = null)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _handler = new AuthenticationHeaderHandler();

            var client = httpClient ?? new HttpClient(_handler)
            {
                BaseAddress = configuration.BaseAddress
            };

            Server = RestService.For<IServerApi>(client);
            Permissions = RestService.For<IPermissionsApi>(client);
            Sessions = RestService.For<ISessionsApi>(client);
        }

        public ISessionsApi Sessions { get; }

        public void Register(string name, string password, string system)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            //var request = new RestRequest(_configuration.GetApiUrl("identities"))
            //{
            //    Method = Method.POST,
            //    RequestFormat = DataFormat.Json
            //};

            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(new
            //{
            //    name,
            //    password,
            //    system
            //});

            //var response = GetResponse(request);

            //if (!response.IsSuccessful)
            //{
            //    throw new ApiException($"[{response.StatusDescription}] : {response.Content}");
            //}
        }

        public void Logout()
        {
            lock (Lock)
            {
                if (!this.HasSession())
                {
                    return;
                }

                var response = Sessions.Delete().Result;

                if (!response.IsSuccessStatusCode)
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
                if (this.HasSession())
                {
                    return;
                }

                var response = Sessions.Post(new RegisterSessionRequest
                {
                    IdentityName = _configuration.IdentityName, 
                    Password = _configuration.Password
                }).Result;

                RegisterSessionResponse = response.Content;

                if (!response.IsSuccessStatusCode ||
                    RegisterSessionResponse == null ||
                    !RegisterSessionResponse.Success ||
                    !RegisterSessionResponse.Token.HasValue)
                {
                    throw new ApiException(Resources.LoginException);
                }

                _handler.Token = RegisterSessionResponse.Token.ToString();
            }
        }

        public void Activate(string name, DateTime dateActivated)
        {
            Activate(new { name, dateActivated });
        }

        public void Activate(Guid id, DateTime dateActivated)
        {
            Activate(new { id, dateActivated });
        }

        public Guid GetPasswordResetToken(string name)
        {
            //var request = new RestRequest(_configuration.GetApiUrl("identities/getpasswordresettoken"))
            //{
            //    Method = Method.POST,
            //    RequestFormat = DataFormat.Json
            //};

            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(new {name});

            //var response = GetResponse(request);

            //if (!response.IsSuccessful)
            //{
            //    throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            //}

            //var passwordResetToken = response.AsDynamic().passwordResetToken;

            //return passwordResetToken == null ? Guid.Empty : (Guid) passwordResetToken;
            throw new NotImplementedException();
        }

        public void ResetPassword(string name, Guid passwordResetToken, string password)
        {
            //var request = new RestRequest(_configuration.GetApiUrl("identities/resetpassword"))
            //{
            //    Method = Method.POST,
            //    RequestFormat = DataFormat.Json
            //};

            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(new {name, passwordResetToken, password});

            //var response = GetResponse(request);

            //if (!response.IsSuccessful)
            //{
            //    throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            //}
        }

        public RegisterSessionResponse RegisterSessionResponse { get; private set; }

        public IServerApi Server { get; }
        public IPermissionsApi Permissions { get; }

        //public RegisterSessionResult RegisterSession(string identityName)
        //{
        //    Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

        //    var request = new RestRequest(_configuration.GetApiUrl("sessions/request"))
        //    {
        //        Method = Method.POST,
        //        RequestFormat = DataFormat.Json
        //    };

        //    request.AddHeader("content-type", "application/json");
        //    request.AddJsonBody(new {identityName});

        //    var response = GetResponse(request);

        //    if (!response.IsSuccessful)
        //    {
        //        throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
        //    }

        //    var content = response.AsDynamic();

        //    if (content.success != null && (bool) content.success)
        //    {
        //        var permissions = new List<string>();

        //        foreach (var permission in content.permissions)
        //        {
        //            permissions.Add(permission.permission.ToString());
        //        }

        //        return RegisterSessionResult.Success(identityName, (Guid) content.token,
        //            (DateTime) content.tokenExpiryDate, permissions);
        //    }

        //    return RegisterSessionResult.Failure();
        //}

        private void ResetSession()
        {
            RegisterSessionResponse = null;
        }

        public void Activate(dynamic model)
        {
            //var request = new RestRequest(_configuration.GetApiUrl("identities/activate"))
            //{
            //    Method = Method.POST,
            //    RequestFormat = DataFormat.Json
            //};

            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(model);

            //var response = GetResponse(request);

            //if (!response.IsSuccessful)
            //{
            //    throw new ApiException($@"[{response.StatusDescription}] : {response.Content}");
            //}
        }
    }
}