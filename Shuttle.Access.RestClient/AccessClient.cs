using System;
using System.Net.Http;
using Refit;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.RestClient.v1;
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
            Identities = RestService.For<IIdentitiesApi>(client);
            Roles = RestService.For<IRolesApi>(client);
        }

        public ISessionsApi Sessions { get; }
        public IIdentitiesApi Identities { get; }
        public IRolesApi Roles { get; }

        public IAccessClient Logout()
        {
            lock (Lock)
            {
                if (!this.HasSession())
                {
                    return this;
                }

                var response = Sessions.Delete().Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiException(Access.Resources.LogoutException); 
                }

                ResetSession();
            }

            return this;
        }

        public IAccessClient Login()
        {
            lock (Lock)
            {
                if (this.HasSession())
                {
                    return this;
                }

                var response = Sessions.Post(new RegisterSession
                {
                    IdentityName = _configuration.IdentityName, 
                    Password = _configuration.Password
                }).Result;

                if (!response.IsSuccessStatusCode ||
                    response.Content == null)
                {
                    throw new ApiException(Access.Resources.LoginException);
                }

                Token = response.Content.Token;
                _handler.Token = Token.Value.ToString("n");
            }

            return this;
        }

        public Guid? Token { get; private set; }


        public IServerApi Server { get; }
        public IPermissionsApi Permissions { get; }

        private void ResetSession()
        {
            Token = null;
        }
    }
}