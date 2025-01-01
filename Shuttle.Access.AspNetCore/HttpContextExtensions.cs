using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;
using System.Text.RegularExpressions;

namespace Shuttle.Access.AspNetCore;

public static class HttpContextExtensions
{
    private static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);

    public static SessionTokenResult GetAccessSessionToken(this HttpContext context)
    {
        Guard.AgainstNull(context);

        try
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();

            if (header == null)
            {
                return SessionTokenResult.Failure(new UnauthorizedResult());
            }

            if (!header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
            {
                return SessionTokenResult.Failure(new UnauthorizedResult());
            }

            var match = TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

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
}