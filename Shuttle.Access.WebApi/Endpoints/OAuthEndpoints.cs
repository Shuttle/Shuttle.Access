using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.OAuth;

namespace Shuttle.Access.WebApi.Endpoints;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth", async (IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, string state, string code) =>
            {
                if (string.IsNullOrWhiteSpace(state) ||
                    !Guid.TryParse(state, out var requestId))
                {
                    return Results.BadRequest();
                }

                var grant = await oauthGrantRepository.GetAsync(requestId);

                var data = await oauthService.GetDataAsync(grant, code);

            if (data == null)
            {
                return Results.BadRequest();
            }

            var options = oauthOptions.Get(grant.ProviderName);

                var email =  GetPropertyValue(data, options.EMailPropertyName);

                if (email == null)
                {
                    return Results.BadRequest();
                }

                return Results.Ok();
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static object? GetPropertyValue(dynamic obj, string propertyName)
    {
        Type objType = obj.GetType();
        var property = objType.GetProperty(propertyName);
        return property?.GetValue(obj, null);
    }
}