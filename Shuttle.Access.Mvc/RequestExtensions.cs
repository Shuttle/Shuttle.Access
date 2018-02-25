using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public static class RequestExtensions
    {
        public static SessionTokenResult GetSessionToken(this HttpRequest request)
        {
            Guard.AgainstNull(request, nameof(request));

            try
            {
                var requestHeaders = request.Headers["access-sessiontoken"];

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