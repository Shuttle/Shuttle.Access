using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shuttle.Access.AspNetCore;

public static class HttpResponseExtensions
{
    extension(HttpResponse httpResponse)
    {
        public async Task<AuthenticateResult> GetTenantIdInvalidAuthenticateResultAsync()
        {
            httpResponse.StatusCode = StatusCodes.Status401Unauthorized;

            const string message = "Invalid GUID received for header 'Shuttle-Access-Tenant-Id'.";

            await httpResponse.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = message
            });

            return AuthenticateResult.Fail(message);
        }
    }
}