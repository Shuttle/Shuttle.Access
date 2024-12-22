using System;
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

    public async ValueTask<bool> ContainsAsync(Guid token)
    {
        var result = Contains(token);

        if (!result)
        {
            await CacheAsync(token);

            return Contains(token);
        }

        return true;
    }

    public async ValueTask<bool> HasPermissionAsync(Guid token, string permission)
    {
        if (!Contains(token))
        {
            await CacheAsync(token);
        }

        return HasPermission(token, permission);
    }

    public new void Remove(Guid token)
    {
        base.Remove(token);
    }

    private async Task CacheAsync(Guid token)
    {
        if (Contains(token))
        {
            return;
        }

        var session = await _accessClient.Sessions.GetAsync(token);

        if (session is { IsSuccessStatusCode: true, Content: not null })
        {
            Cache(token, session.Content.Permissions, _accessOptions.SessionDuration);
        }
    }
}