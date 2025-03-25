using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class RestSessionCache : SessionCache, ISessionCache
{
    private readonly IAccessClient _accessClient;
    private readonly AccessOptions _accessOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RestSessionCache(IOptions<AccessOptions> accessOptions, IAccessClient accessClient)
    {
        _accessOptions = Guard.AgainstNull(accessOptions).Value;
        _accessClient = Guard.AgainstNull(accessClient);
    }

    public async Task<Messages.v1.Session?> FindAsync(string identityName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityName);

            if (session != null)
            {
                return session;
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                IdentityName = identityName,
                ShouldIncludePermissions = true
            });

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null })
            {
                if (sessionResponse.Content.Count() == 1)
                {
                    return AddSession(null, sessionResponse.Content.Single());
                }

                if (sessionResponse.Content.Count() > 1)
                {
                    throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "IdentityName", identityName));
                }
            }
            else
            {
                var registrationResponse = await _accessClient.Sessions.PostAsync(new RegisterSession
                {
                    IdentityName = identityName
                });

                if (registrationResponse is { IsSuccessStatusCode: true, Content: not null })
                {
                    return AddSession(registrationResponse.Content.Token, registrationResponse.Content);
                }
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync(Guid identityGuid, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> HasPermissionAsync(Guid identityId, string permission, CancellationToken cancellationToken = default)
    {
        var session = await FindAsync(identityId, cancellationToken);

        return session != null && HasPermission(session.IdentityId, permission, _accessOptions.AdministratorPermissionName);
    }

    public async Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (Find(session.IdentityId) != null)
            {
                return;
            }

            Add(token, session);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindByTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = FindByToken(token);

            if (session != null)
            {
                return session;
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                Token = token,
                ShouldIncludePermissions = true
            });

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null })
            {
                if (sessionResponse.Content.Count() == 1)
                {
                    return AddSession(token, sessionResponse.Content.Single());
                }

                if (sessionResponse.Content.Count() > 1)
                {
                    var tokenValue = token.ToString("N");

                    throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "token", $"{tokenValue[..4]}****-****-****-****-********{tokenValue[^4..]}"));
                }
            }
            
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityId);

            if (session != null)
            {
                return session;
            }

            var sessionResponse = await _accessClient.Sessions.PostSearchAsync(new()
            {
                IdentityId = identityId,
                ShouldIncludePermissions = true
            });

            if (sessionResponse is { IsSuccessStatusCode: true, Content: not null })
            {
                if (sessionResponse.Content.Count() == 1)
                {
                    return AddSession(null, sessionResponse.Content.Single());
                }

                if (sessionResponse.Content.Count() > 1)
                {
                    throw new InvalidOperationException(string.Format(Resources.UnexpectedMultipleSessionsException, "IdentityId", identityId.ToString("D")));
                }
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private Messages.v1.Session AddSession(Guid? token, SessionResponse sessionResponse)
    {
        return Add(token, new()
        {
            IdentityId = sessionResponse.IdentityId,
            IdentityName = sessionResponse.IdentityName,
            DateRegistered = sessionResponse.DateRegistered,
            ExpiryDate = sessionResponse.ExpiryDate,
            Permissions = sessionResponse.Permissions.ToList()
        });
    }
}