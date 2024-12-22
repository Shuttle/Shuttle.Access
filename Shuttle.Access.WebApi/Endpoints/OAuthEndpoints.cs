using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.OAuth;

namespace Shuttle.Access.WebApi.Endpoints;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/providers", (IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions) =>
            {
                var result = new List<OAuthProvider>();

                foreach (var providerName in accessOptions.Value.OAuthProviderNames)
                {
                    var oauthProvider = new OAuthProvider
                    {
                        Name = providerName
                    };

                    if (File.Exists(Path.Combine(accessOptions.Value.OAuthProviderSvgFolder, $"{providerName}.svg")))
                    {
                        oauthProvider.Svg = File.ReadAllText(Path.Combine(accessOptions.Value.OAuthProviderSvgFolder, $"{providerName}.svg"));
                    }

                    result.Add(oauthProvider);
                }

                return Results.Ok(result);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{providerName}", async (IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, string providerName) =>
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

                return Results.Ok(new
                {
                    AuthorizationUrl = providerOptions.AuthorizationUrl
                                          .Replace("__ClientId__", providerOptions.ClientId)
                                          .Replace("__ClientSecret__", providerOptions.ClientSecret)
                                          .Replace("__CodeChallengeMethod__", providerOptions.CodeChallengeMethod)
                                          .Replace("__Scope__", providerOptions.Scope)
                                          .Replace("__RedirectUri__", accessOptions.Value.OAuthRedirectUri)
                                      + $"&state={grant.Id}"
                });
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{code}", async (IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IDatabaseContextFactory databaseContextFactory, IMediator mediator, string state, string code) =>
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

                var requestRegistration = registerSession.Result == SessionRegistrationResult.UnknownIdentity &&
                                                          accessOptions.Value.OAuthRegisterUnknownIdentities;
                if (requestRegistration)
                {
                    var requestIdentityRegistration = new RequestIdentityRegistration(new()
                    {
                        Name = email
                    })
                    .Allowed(grant.ProviderName);

                    await mediator.SendAsync(requestIdentityRegistration);
                }

                var sessionResponse = new SessionResponse
                {
                    Result = registerSession.Result.ToString(),
                    RegistrationRequested = requestRegistration
                };

                if (registerSession.HasSession)
                {
                    sessionResponse.IdentityName = registerSession.Session!.IdentityName;
                    sessionResponse.Token = registerSession.Session.Token;
                    sessionResponse.TokenExpiryDate = registerSession.Session.ExpiryDate;
                    sessionResponse.Permissions = registerSession.Session.Permissions.ToList();
                }

                return Results.Ok(sessionResponse);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}