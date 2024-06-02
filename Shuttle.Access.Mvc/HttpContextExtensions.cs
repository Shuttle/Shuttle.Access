using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public static class HttpContextExtensions
    {
        public static readonly string AuthorizationScheme = "Bearer";
        private static readonly char[] Space = new[] {' '};

        public static SessionTokenResult GetAccessSessionToken(this HttpContext context)
        {
            Guard.AgainstNull(context, nameof(context));

            try
            {
                var headers = context.Request.Headers["Authorization"];

                if (headers.Count != 1)
                {
                    return SessionTokenResult.Failure(new UnauthorizedResult());
                }

                var values = headers[0].Split(Space);

                return values.Length == 2 &&
                       values[0].Equals(AuthorizationScheme) &&
                       Guid.TryParse(values[1], out var sessionToken)
                    ? SessionTokenResult.Success(sessionToken)
                    : SessionTokenResult.Failure(new UnauthorizedResult());
            }
            catch
            {
                throw new Exception("Could not retrieve the session token.");
            }
        }
    }
}