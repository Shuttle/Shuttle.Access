using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public abstract class CachedAccessService 
    {
        private MemoryCache _sessions = new MemoryCache(new MemoryCacheOptions());
        private readonly object _lock = new object();

        protected bool Contains(Guid token)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(token, out _);
            }
        }

        protected void Cache(Guid token, IEnumerable<string> permissions, TimeSpan slidingExpiration)
        {
            Guard.AgainstNull(permissions, nameof(permissions));

            lock (_lock)
            {
                using (var entry = _sessions.CreateEntry(token))
                {
                    entry.Value = new List<string>(permissions);
                    entry.SlidingExpiration = slidingExpiration;
                }
            }
        }

        protected bool HasPermission(Guid token, string permission)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(token, out List<string> permissions))
                {
                    return permissions.Contains(permission) || permissions.Contains("*");
                }
                else
                {
                    return false;
                }
            }
        }

        protected void Remove(Guid token)
        {
            lock (_lock)
            {
                _sessions.Remove(token);
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                _sessions.Dispose();
                _sessions = new MemoryCache(new MemoryCacheOptions());
            }
        }
    }
}