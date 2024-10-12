using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public abstract class CachedAccessService
    {
        private readonly object _lock = new();
        private readonly MemoryCache _sessions = new(new MemoryCacheOptions());

        protected void Cache(Guid token, IEnumerable<string> permissions, TimeSpan slidingExpiration)
        {
            lock (_lock)
            {
                using (var entry = _sessions.CreateEntry(token))
                {
                    entry.Value = new List<string>(Guard.AgainstNull(permissions));
                    entry.SlidingExpiration = slidingExpiration;
                }
            }
        }

        protected bool Contains(Guid token)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(token, out _);
            }
        }

        public void Flush(Guid token)
        {
            lock (_lock)
            {
                _sessions.Remove(token);
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

                return false;
            }
        }

        protected void Remove(Guid token)
        {
            lock (_lock)
            {
                _sessions.Remove(token);
            }
        }
    }
}