using Asp.Versioning;
using Asp.Versioning.Builder;
using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.OAuth;

namespace Shuttle.Access.WebApi.Endpoints;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{providerName}", async (HttpContext httpContext, IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, string providerName) =>
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return Results.BadRequest("No provider name has been specified.");
                }

                if (!accessOptions.Value.OAuthProviderNames.Any())
                {
                    return Results.BadRequest("No OAuth providers have been configured.");
                }

                if (!accessOptions.Value.OAuthProviderNames.Contains(providerName))
                {
                    return Results.BadRequest($"No provider named '{providerName}' has been configured.  Available providers: {string.Join(',', accessOptions.Value.OAuthProviderNames)}");
                }

                var grant = await oauthService.RegisterAsync(providerName);

                var providerOptions = oauthOptions.Get(providerName);

                var redirectUrl = providerOptions.AuthorizationUrl
                    .Replace("__ClientId__", providerOptions.ClientId)
                    .Replace("__ClientSecret__", providerOptions.ClientSecret)
                    .Replace("__CodeChallengeMethod__", providerOptions.CodeChallengeMethod)
                    .Replace("__Scope__", providerOptions.Scope)
                    .Replace("__RedirectUri__", $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}/v1/oauth/session")
                                  + $"&state={grant.Id}";

                return Results.Redirect(redirectUrl);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session", async (IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IDatabaseContextFactory databaseContextFactory, IMediator mediator, IServiceBus serviceBus, string state, string code) =>
            {
                if (string.IsNullOrWhiteSpace(state) ||
                    !Guid.TryParse(state, out var requestId))
                {
                    return Results.BadRequest();
                }

                var grant = await oauthGrantRepository.GetAsync(requestId);
                var data = await oauthService.GetDataAsync(grant, code);
                var options = oauthOptions.Get(grant.ProviderName);
                var email = data.GetProperty(options.EMailPropertyName).ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return Results.BadRequest($"No e-mail address property '{options.EMailPropertyName}' was returned from the OAuth provider.");
                }

                var registerSession = new Application.RegisterSession(email).UseDirect();

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await mediator.SendAsync(registerSession);
                }

                if (registerSession.Result == SessionRegistrationResult.UnknownIdentity &&
                    accessOptions.Value.OAuthRegisterUnknownIdentities)
                {
                    var requestIdentityRegistration = new RequestIdentityRegistration(new()
                    {
                        Name = email
                    })
                    .Allowed(grant.ProviderName);

                    await mediator.SendAsync(requestIdentityRegistration);
                }

                return registerSession.HasSession
                    ? Results.Ok(new SessionRegistered
                    {
                        IdentityName = registerSession.Session.IdentityName,
                        Token = registerSession.Session.Token,
                        TokenExpiryDate = registerSession.Session.ExpiryDate,
                        Permissions = registerSession.Session.Permissions.ToList()
                    })
                    : Results.BadRequest(registerSession.Result);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}