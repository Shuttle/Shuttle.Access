using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class HttpContextExtensions
{
    public static SessionTokenResult GetAccessSessionToken(this HttpContext context)
    {
        Guard.AgainstNull(context);

        try
        {
            var sessionTokenClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == AccessAuthenticationHandler.SessionTokenClaimType)?.Value;

            if (sessionTokenClaim != null)
            {
                return Guid.TryParse(sessionTokenClaim, out var token) 
                    ? SessionTokenResult.Success(token)
                    : SessionTokenResult.Failure(new UnauthorizedResult());
            }

            var header = context.Request.Headers["Authorization"].FirstOrDefault();

            if (header == null)
            {
                return SessionTokenResult.Failure(new UnauthorizedResult());
            }

            if (!header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
            {
                return SessionTokenResult.Failure(new UnauthorizedResult());
            }

            var match = AccessAuthenticationHandler.TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

            if (!match.Success)
            {
                return SessionTokenResult.Failure(new UnauthorizedResult());
            }

            return !Guid.TryParse(match.Groups["token"].Value, out var sessionToken) 
                ? SessionTokenResult.Failure(new UnauthorizedResult()) 
                : SessionTokenResult.Success(sessionToken);
        }
        catch
        {
            throw new("Could not retrieve the session token.");
        }
    }

    public static void SetPrincipalAccessSessionToken(this HttpContext context, Guid sessionToken)
    {
        Guard.AgainstNull(context);

        var identity = new ClaimsIdentity(context.User.Identity);

        identity.AddClaim(new(AccessAuthenticationHandler.SessionTokenClaimType, sessionToken.ToString()));

        context.User = new(identity);
    }
}