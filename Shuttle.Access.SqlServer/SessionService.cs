using Shuttle.Access.Query;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionService(ISessionCache sessionCache, IHashingService hashingService, ISessionRepository sessionRepository)
    : ISessionService
{
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_sessionCache.Find(new()
                {
                    IdentityId = session.IdentityId
                }) != null)
            {
                return;
            }

            _sessionCache.Add(token, session);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = _sessionCache.Find(specification);

            if (session != null)
            {
                return session;
            }

            var sessionSpecification = new SessionSpecification();

            if (specification.Id.HasValue)
            {
                sessionSpecification.AddId(specification.Id.Value);
            }

            if (specification.Token.HasValue)
            {
                sessionSpecification.WithToken(_hashingService.Sha256(specification.Token.Value.ToString("D")));
            }

            if (specification.TenantId.HasValue)
            {
                sessionSpecification.WithTenantId(specification.TenantId.Value);
            }

            if (specification.IdentityId.HasValue)
            {
                sessionSpecification.WithIdentityId(specification.IdentityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(specification.IdentityName))
            {
                sessionSpecification.WithIdentityName(specification.IdentityName);
            }

            return Add(specification.Token, await _sessionRepository.FindAsync(sessionSpecification, cancellationToken));
        }
        finally
        {
            _lock.Release();
        }
    }

    private Messages.v1.Session? Add(Guid? token, Session? session)
    {
        if (session == null)
        {
            return null;
        }

        return sessionCache.Add(token, new()
        {
            TenantId = session.TenantId,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.Permissions.Select(item => item.Name).ToList()
        });
    }
}