using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.OAuth;
using Shuttle.OAuth.GitHub;

namespace Shuttle.Access.WebApi.Endpoints;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/github", async (IOptions<AccessOptions> accessOptions, IOAuthProviderService oauthProviderService, string code) =>
            {
                var emailAddress = GitHubData.EMailAddress(await oauthProviderService.Get("GitHub").GetDataDynamicAsync(code));

                return Results.Ok();
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}