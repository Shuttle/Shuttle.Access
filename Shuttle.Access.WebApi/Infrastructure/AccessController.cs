using System;
using Microsoft.AspNetCore.Mvc;

namespace Shuttle.Access.WebApi
{
    [Route("api/[controller]")]
    public class AccessController : Controller
    {
        protected SessionTokenResult GetSessionToken()
        {
            try
            {
                var requestHeaders = Request.Headers["access-sessiontoken"];

                if (requestHeaders.Count == 1)
                {
                    var sessionTokenValue = requestHeaders[0];

                    return !Guid.TryParse(sessionTokenValue, out var sessionToken)
                        ? SessionTokenResult.Failure(Unauthorized())
                        : SessionTokenResult.Success(sessionToken);
                }

                return SessionTokenResult.Failure(Unauthorized());
            }
            catch
            {
                throw new Exception("Could not retrieve the session token");
            }
        }
    }
}