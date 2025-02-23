using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class RestAccessService : CachedAccessService, IAccessService
{
    private readonly IAccessClient _accessClient;
    private readonly AccessOptions _accessOptions;

    public RestAccessService(IOptions<AccessOptions> accessOptions, IAccessClient accessClient)
    {
        _accessOptions = Guard.AgainstNull(accessOptions).Value;
        _accessClient = Guard.AgainstNull(accessClient);
    }

    public async ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default)
    {
        var result = await base.ContainsAsync(token);

        if (!result)
        {
            await CacheAsync(token);

            return await base.ContainsAsync(token);
        }

        return true;
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await base.FlushAsync();
    }

    public async ValueTask<bool> HasPermissionAsync(Guid token, string permission, CancellationToken cancellationToken = default)
    {
        if (!await base.ContainsAsync(token))
        {
            await CacheAsync(token);
        }

        return await base.HasPermissionAsync(token, permission);
    }

    public async Task RemoveAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await base.RemoveAsync(token);
    }

    public async Task<Session?> FindSessionAsync(Guid token, CancellationToken cancellationToken = default)
    {
        var sessionResponse = await _accessClient.Sessions.GetAsync(token);

        if (sessionResponse is { IsSuccessStatusCode: true, Content: not null })
        {
            var session = sessionResponse.Content;

            return new Session(session.Token, session.IdentityId, session.IdentityName, session.DateRegistered, session.ExpiryDate);
        }

        return null;
    }

    private async Task CacheAsync(Guid token)
    {
        if (await base.ContainsAsync(token))
        {
            return;
        }

        var session = await _accessClient.Sessions.GetAsync(token);

        if (session is { IsSuccessStatusCode: true, Content: not null })
        {
            await CacheAsync(token, session.Content.Permissions, _accessOptions.SessionDuration);
        }
    }
}