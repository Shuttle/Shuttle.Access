using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public static class HttpContextExtensions
    {
        public static readonly string AccessSessionTokenHeaderName = "access-session-token";

        public static SessionTokenResult GetAccessSessionToken(this HttpContext context)
        {
            Guard.AgainstNull(context, nameof(context));

            try
            {
                var requestHeaders = context.Request.Headers[AccessSessionTokenHeaderName];

                if (requestHeaders.Count == 1)
                {
                    var sessionTokenValue = requestHeaders[0];

                    return !Guid.TryParse(sessionTokenValue, out var sessionToken)
                        ? SessionTokenResult.Failure(new UnauthorizedResult())
                        : SessionTokenResult.Success(sessionToken);
                }

                return SessionTokenResult.Failure(new UnauthorizedResult());
            }
            catch
            {
                throw new Exception("Could not retrieve the session token.");
            }
        }
    }
}