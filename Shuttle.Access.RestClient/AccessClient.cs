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

        public AccessClient(IAccessClientConfiguration configuration, HttpClient httpClient)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(httpClient, nameof(httpClient));

            _configuration = configuration;

            Server = RestService.For<IServerApi>(httpClient);
            Permissions = RestService.For<IPermissionsApi>(httpClient);
            Sessions = RestService.For<ISessionsApi>(httpClient);
            Identities = RestService.For<IIdentitiesApi>(httpClient);
            Roles = RestService.For<IRolesApi>(httpClient);
        }

        public ISessionsApi Sessions { get; }
        public IIdentitiesApi Identities { get; }
        public IRolesApi Roles { get; }

        public IAccessClient DeleteSession()
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
                    throw new ApiException(Access.Resources.DeleteSessionException); 
                }

                ResetSession();
            }

            return this;
        }

        public IAccessClient RegisterSession()
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
                    throw new ApiException(Access.Resources.RegisterSessionException);
                }

                Token = response.Content.Token;
                TokenExpiryDate = response.Content.TokenExpiryDate.Subtract(TimeSpan.FromMinutes(1));
            }

            return this;
        }

        public DateTime? TokenExpiryDate { get; private set; }
        public Guid? Token { get; private set; }

        public IServerApi Server { get; }
        public IPermissionsApi Permissions { get; }

        private void ResetSession()
        {
            Token = null;
        }
    }
}