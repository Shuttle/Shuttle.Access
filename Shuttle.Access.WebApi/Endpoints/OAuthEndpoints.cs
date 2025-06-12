using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.OAuth;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/providers", (IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions) =>
            {
                Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
                Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);

                var result = new List<OAuthProvider>();

                foreach (var oauthProviderOptions in oauthOptions.Value.Providers)
                {
                    var oauthProvider = new OAuthProvider
                    {
                        Name = oauthProviderOptions.Name
                    };

                    var path = Path.Combine(accessOptions.Value.ExtensionFolder, "OAuth", $"{oauthProviderOptions.Name}.svg");

                    if (File.Exists(path))
                    {
                        oauthProvider.Svg = File.ReadAllText(path);
                    }

                    result.Add(oauthProvider);
                }

                return Results.Ok(result);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{providerName}/{applicationName?}", async (ILogger<OAuthService> logger, IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, string providerName, string? applicationName, [FromQuery] string? redirectUri) =>
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return Results.BadRequest("No provider name has been specified.");
                }

                logger.LogDebug($"[oauth/register] : provider = '{providerName}'");

                var data = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(applicationName))
                {
                    if (Guard.AgainstNull(accessOptions.Value).KnownApplications.FirstOrDefault(item => item.Name.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase)) == null)
                    {
                        return Results.BadRequest($"Unknown application name '{applicationName}'.");
                    }

                    data.Add("ApplicationName", applicationName);
                }

                var grant = await oauthService.RegisterAsync(providerName, data);

                logger.LogDebug($"[oauth/registered] : grant id = '{grant.Id}' / provider = '{providerName}' / application name = '{(string.IsNullOrWhiteSpace(applicationName) ? string.Empty : applicationName)}'");

                var oauthOptionsValue = Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);
                var oauthProviderOptions = oauthOptionsValue.GetProviderOptions(providerName);

                var authorizationUrl = new StringBuilder(oauthProviderOptions.Authorize.Url);

                authorizationUrl.Append($"?response_type=code&client_id={oauthProviderOptions.Authorize.ClientId}");
                authorizationUrl.Append($"&scope={oauthProviderOptions.Scope}");
                authorizationUrl.Append($"&redirect_uri={(string.IsNullOrWhiteSpace(redirectUri) ? oauthOptionsValue.DefaultRedirectUri : redirectUri)}");

                if (!string.IsNullOrWhiteSpace(oauthProviderOptions.Authorize.CodeChallengeMethod))
                {
                    if (string.IsNullOrWhiteSpace(grant.CodeChallenge))
                    {
                        return Results.BadRequest($"No 'CodeChallenge' has been generated for code challenge method '{oauthProviderOptions.Authorize.CodeChallengeMethod}'.");
                    }

                    logger.LogDebug($"[oauth/code challenge] : grant id = '{grant.Id}' / provider = '{providerName}'");

                    authorizationUrl.Append($"&code_challenge_method={oauthProviderOptions.Authorize.CodeChallengeMethod}&code_challenge={grant.CodeChallenge}");
                }

                return Results.Ok(new
                {
                    AuthorizationUrl = authorizationUrl + $"&state={grant.Id}"
                });
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{code}", async (ILogger<OAuthService> logger, IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IDatabaseContextFactory databaseContextFactory, IMediator mediator, string state, string code) =>
            {
                if (string.IsNullOrWhiteSpace(state) || !Guid.TryParse(state, out var requestId))
                {
                    return Results.BadRequest();
                }

                logger.LogDebug($"[oauth/retrieve] : grant id = '{requestId}'");

                var grant = await oauthGrantRepository.GetAsync(requestId);

                logger.LogDebug($"[oauth/identity request] : grant id = '{requestId}'");

                var data = await oauthService.GetDataAsync(grant, code);
                var oauthProviderOptions = Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value).GetProviderOptions(grant.ProviderName);

                if (string.IsNullOrWhiteSpace(oauthProviderOptions.Data.IdentityPropertyName))
                {
                    return Results.Problem($"The 'Data.IdentityPropertyName' is empty for the '{grant.ProviderName}' provider options.");
                }

                var identity = data.GetProperty(oauthProviderOptions.Data.IdentityPropertyName).ToString();

                if (string.IsNullOrWhiteSpace(identity))
                {
                    return Results.BadRequest($"No identity property '{oauthProviderOptions.Data.IdentityPropertyName}' was returned from the data endpoint provider.");
                }

                logger.LogDebug($"[oauth/identity] : grant id = '{requestId}' / identity = '{identity}'");

                var registerSession = new RegisterSession(identity).UseDirect();

                if (grant.HasData("ApplicationName"))
                {
                    var applicationName = grant.GetData("ApplicationName");

                    var knownApplicationOptions = Guard.AgainstNull(accessOptions.Value).KnownApplications.FirstOrDefault(item => item.Name.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase));

                    if (knownApplicationOptions == null)
                    {
                        return Results.BadRequest($"Unknown application name '{applicationName}'.");
                    }

                    registerSession.WithKnownApplicationOptions(knownApplicationOptions);
                }

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
                            Name = identity,
                            Activated = true
                        })
                        .Allowed(grant.ProviderName);

                    await mediator.SendAsync(requestIdentityRegistration);
                }

                var sessionResponse = new SessionResponse
                {
                    Result = registerSession.Result.ToString(),
                    RegistrationRequested = requestRegistration,
                    IdentityName = identity
                };

                if (registerSession.HasSession)
                {
                    sessionResponse.IdentityId = registerSession.Session!.IdentityId;
                    sessionResponse.IdentityName = registerSession.Session!.IdentityName;
                    sessionResponse.Token = registerSession.SessionToken!.Value;
                    sessionResponse.ExpiryDate = registerSession.Session.ExpiryDate;
                    sessionResponse.Permissions = registerSession.Session.Permissions.ToList();
                    sessionResponse.SessionTokenExchangeUrl = registerSession.SessionTokenExchangeUrl;
                    sessionResponse.DateRegistered = registerSession.Session.DateRegistered;
                }

                return Results.Ok(sessionResponse);
            })
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}