using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public abstract class CachedAccessService
{
    private readonly SemaphoreSlim _lock = new(1,1);
    private readonly MemoryCache _sessions = new(new MemoryCacheOptions());

    protected async Task CacheAsync(Guid token, IEnumerable<string> permissions, TimeSpan slidingExpiration)
    {
        await _lock.WaitAsync();

        if (_sessions.TryGetValue(token, out _))
        {
            return;
        }

        try
        {
            using (var entry = _sessions.CreateEntry(token))
            {
                entry.Value = new List<string>(Guard.AgainstNull(permissions));
                entry.SlidingExpiration = slidingExpiration;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    protected async ValueTask<bool> ContainsAsync(Guid token)
    {
        await _lock.WaitAsync();

        try
        {
            return _sessions.TryGetValue(token, out _);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task FlushAsync()
    {
        await _lock.WaitAsync();

        try
        {
            _sessions.Clear();
        }
        finally
        {
            _lock.Release();
        }
    }

    protected async ValueTask<bool> HasPermissionAsync(Guid token, string permission)
    {
        await _lock.WaitAsync();

        try
        {
            if (_sessions.TryGetValue(token, out List<string>? permissions))
            {
                return permissions != null && (permissions.Contains(permission) || permissions.Contains("*"));
            }

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    protected async Task RemoveAsync(Guid token)
    {
        await _lock.WaitAsync();
        
        try
        {
            _sessions.Remove(token);
        }
        finally
        {
            _lock.Release();
        }
    }
}