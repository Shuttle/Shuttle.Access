using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Refit;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.RestClient.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AccessClient : IAccessClient
{
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private readonly AccessClientOptions _accessClientOptions;

    public AccessClient(IOptions<AccessClientOptions> accessClientOptions, HttpClient httpClient)
    {
        _accessClientOptions = Guard.AgainstNull(accessClientOptions).Value;

        Guard.AgainstNull(httpClient);

        Server = RestService.For<IServerApi>(httpClient);
        Permissions = RestService.For<IPermissionsApi>(httpClient);
        Sessions = RestService.For<ISessionsApi>(httpClient);
        Identities = RestService.For<IIdentitiesApi>(httpClient);
        Roles = RestService.For<IRolesApi>(httpClient);
    }

    public ISessionsApi Sessions { get; }
    public IIdentitiesApi Identities { get; }
    public IRolesApi Roles { get; }

    public async Task<IAccessClient> DeleteSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Lock.WaitAsync(cancellationToken);

            if (!this.HasSession())
            {
                return this;
            }

            var response = await Sessions.DeleteAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(Access.Resources.DeleteSessionException);
            }

            ResetSession();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Lock.Release();
        }

        return this;
    }

    public async Task<IAccessClient> RegisterSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Lock.WaitAsync(cancellationToken);

            if (this.HasSession())
            {
                return this;
            }

            var response = await Sessions.PostAsync(new RegisterSession
            {
                IdentityName = _accessClientOptions.IdentityName,
                Password = _accessClientOptions.Password
            });

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                throw new ApiException(Access.Resources.RegisterSessionException);
            }

            Token = response.Content.Token;
            TokenExpiryDate = response.Content.TokenExpiryDate;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Lock.Release();
        }

        return this;
    }

    public DateTimeOffset? TokenExpiryDate { get; private set; }
    public Guid? Token { get; private set; }

    public IServerApi Server { get; }
    public IPermissionsApi Permissions { get; }

    private void ResetSession()
    {
        Token = null;
        TokenExpiryDate = null;
    }
}