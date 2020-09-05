using System.Linq;
using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public static class HttpContextExtensions
    {
        public static readonly string AccessSessionTokenHeaderName = "access-sessiontoken";

        public static string GetAccessSessionToken(this HttpContext httpContext)
        {
            Guard.AgainstNull(httpContext, nameof(httpContext));

            var headers = httpContext.Request.Headers;

            if (!headers.ContainsKey(AccessSessionTokenHeaderName))
            {
                return null;
            }

            var tokens = headers[AccessSessionTokenHeaderName].ToList();

            return tokens.Count != 1 ? null : tokens[0];
        }
    }
}