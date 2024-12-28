using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.OAuth;
using System.Configuration.Provider;

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

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{providerName}", async (ILogger<OAuthService> logger, IOptions <AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, string providerName) =>
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

                logger.LogDebug($"[oauth/register] : provider = '{providerName}'");

                var grant = await oauthService.RegisterAsync(providerName);

                logger.LogDebug($"[oauth/registered] : grant id = '{grant.Id}' / provider = '{providerName}'");

                var providerOptions = oauthOptions.Get(providerName);

                var authorizationUrl = providerOptions.AuthorizationUrl
                    .Replace("__ClientId__", providerOptions.ClientId)
                    .Replace("__ClientSecret__", providerOptions.ClientSecret)
                    .Replace("__CodeChallengeMethod__", providerOptions.CodeChallengeMethod)
                    .Replace("__Scope__", providerOptions.Scope)
                    .Replace("__RedirectUri__", accessOptions.Value.OAuthRedirectUri);

                if (!string.IsNullOrWhiteSpace(providerOptions.CodeChallengeMethod))
                {
                    if (string.IsNullOrWhiteSpace(grant.CodeChallenge))
                    {
                        return Results.BadRequest($"No 'CodeChallenge' has been generated for code challenge method '{providerOptions.CodeChallengeMethod}'.");
                    }

                    logger.LogDebug($"[oauth/code challenge] : grant id = '{grant.Id}' / provider = '{providerName}'");

                    authorizationUrl = authorizationUrl.Replace("__CodeChallenge__", grant.CodeChallenge);
                }

                return Results.Ok(new
                {
                    AuthorizationUrl = authorizationUrl + $"&state={grant.Id}"

                });
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{code}", async (ILogger<OAuthService> logger, IOptions<AccessOptions> accessOptions, IOptionsMonitor<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IDatabaseContextFactory databaseContextFactory, IMediator mediator, string state, string code) =>
            {
                if (string.IsNullOrWhiteSpace(state) ||
                    !Guid.TryParse(state, out var requestId))
                {
                    return Results.BadRequest();
                }

                logger.LogDebug($"[oauth/retrieve] : grant id = '{requestId}'");

                var grant = await oauthGrantRepository.GetAsync(requestId);
                var data = await oauthService.GetDataAsync(grant, code);
                var options = oauthOptions.Get(grant.ProviderName);

                logger.LogDebug($"[oauth/e-mail request] : grant id = '{requestId}'");

                var email = data.GetProperty(options.EMailPropertyName).ToString();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return Results.BadRequest($"No e-mail address property '{options.EMailPropertyName}' was returned from the OAuth provider.");
                }

                logger.LogDebug($"[oauth/e-mail] : grant id = '{requestId}' / e-mail = '{email}'");

                var registerSession = new Application.RegisterSession(email).UseDirect();

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await mediator.SendAsync(registerSession);
                }

                var requestRegistration = registerSession.Result == SessionRegistrationResult.UnknownIdentity && accessOptions.Value.OAuthRegisterUnknownIdentities;

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