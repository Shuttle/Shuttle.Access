using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Mediator;
using Session = Shuttle.Access.Query.Session;

namespace Shuttle.Access.Application;

public class SessionRequestParticipant(IOptions<AccessOptions> accessOptions, IAuthenticationService authenticationService, IHashingService hashingService, ISessionQuery sessionQuery, IIdentityQuery identityQuery)
    : IParticipant<SessionRequest>
{
    public async Task HandleAsync(SessionRequest message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(accessOptions);
        Guard.AgainstNull(authenticationService);
        Guard.AgainstNull(hashingService);
        Guard.AgainstNull(sessionQuery);
        Guard.AgainstNull(identityQuery);

        Session? session;

        var now = DateTimeOffset.UtcNow;
        var sessionExpiryDate = now.Add(accessOptions.Value.SessionDuration);

        switch (message.RequestType)
        {
            case SessionRequestType.Password:
            {
                var authenticationResult = await authenticationService.AuthenticateAsync(message.IdentityName, message.GetPassword(), cancellationToken);

                if (!authenticationResult.Authenticated)
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRequestType.Token:
            {
                var specification = new Session.Specification().WithTokenHash(hashingService.Sha256(message.GetSessionToken().ToString("D")));

                session = (await sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault();

                var application = session?.Tokens.First(item => item.TokenHash.Equals(specification.TokenHash, StringComparison.InvariantCultureIgnoreCase)).Application ?? "Access";

                if (session != null && !session.HasExpired(accessOptions.Value.SessionRenewalTolerance, application))
                {
                    message.Renewed(session);

                    await SaveAsync(message, cancellationToken, sessionExpiryDate);
                }
                else
                {
                    message.Forbidden();
                    return;
                }

                break;
            }
            case SessionRequestType.Direct:
            {
                break;
            }
            default:
            {
                throw new InvalidOperationException(string.Format(Resources.SessionRegistrationTypeNoneException, message.RequestType));
            }
        }

        var identity = (await identityQuery.SearchAsync(new Query.Identity.Specification().WithName(message.IdentityName).IncludeTenants(), cancellationToken)).SingleOrDefault();

        if (identity == null)
        {
            message.UnknownIdentity();
            return;
        }

        var tenants = identity.Tenants
            .Where(item => item.Status == TenantStatus.Active)
            .Select(item => new Query.Tenant
            {
                Id = item.Id,
                Name = item.Name,
                Status = item.Status,
                LogoSvg = item.LogoSvg,
                LogoUrl = item.LogoUrl
            })
            .ToList();

        message.WithTenants(tenants);

        if (message.RequestType == SessionRequestType.Token)
        {
            return;
        }

        // A new token has to be issued here.
        var token = Guid.NewGuid();

        session = (await sessionQuery.SearchAsync(new Session.Specification().WithIdentityName(message.IdentityName), cancellationToken)).FirstOrDefault()
                  ?? new()
                  {
                      Id = Guid.NewGuid(),
                      IdentityId = identity.Id,
                      IdentityName = identity.Name,
                      DateRegistered = now
                  };

        session.IdentityDescription = identity.Description;

        var sessionToken = session.FindSessionToken(message.Application);

        if (sessionToken == null)
        {
            sessionToken = new()
            {
                Id = Guid.NewGuid(),
                DateRegistered = now,
                Application = message.Application
            };

            session.Tokens.Add(sessionToken);
        }

        sessionToken.ExpiryDate = sessionExpiryDate;
        sessionToken.TokenHash = Convert.ToHexString(hashingService.Sha256(token.ToString("D")));

        message.Registered(token, session);

        await SaveAsync(message, cancellationToken, sessionExpiryDate);
    }

    private async Task SaveAsync(SessionRequest message, CancellationToken cancellationToken, DateTimeOffset sessionExpiryDate)
    {
        var session = Guard.AgainstNull(message.Session);

        session.FindSessionToken(message.Application)?.ExpiryDate = sessionExpiryDate;
        session.ExpiryDate = sessionExpiryDate;
        session.Permissions = (await identityQuery.PermissionsAsync(session.IdentityId, cancellationToken)).ToList();

        await sessionQuery.SaveAsync(session, cancellationToken);
    }
}