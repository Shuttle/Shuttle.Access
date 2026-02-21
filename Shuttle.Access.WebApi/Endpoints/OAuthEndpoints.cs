using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.OAuth;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/providers/{group?}", GetProviders)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{providerName}", GetAuthenticateProvider)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{code}", GetSessionStateCode)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> GetSessionStateCode(ILogger<OAuthService> logger, IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IMediator mediator, string state, string code)
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

        var identityName = data.GetProperty(oauthProviderOptions.Data.IdentityPropertyName).ToString();

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return Results.BadRequest($"No identity property '{oauthProviderOptions.Data.IdentityPropertyName}' was returned from the data endpoint provider.");
        }

        logger.LogDebug($"[oauth/identity] : grant id = '{requestId}' / identity = '{identityName}'");

        var registerSession = new RegisterSession(identityName).UseDirect();

        await mediator.SendAsync(registerSession);

        var requestRegistration = registerSession.Result == SessionRegistrationResult.UnknownIdentity && apiOptions.Value.OAuthRegisterUnknownIdentities;

        if (requestRegistration)
        {
            var requestIdentityRegistration = new RequestIdentityRegistration(new() { Name = identityName, Activated = true }).Allowed(grant.ProviderName);

            await mediator.SendAsync(requestIdentityRegistration);
        }

        return Results.Ok(registerSession.GetSessionResponse(requestRegistration));
    }

    private static async Task<IResult> GetAuthenticateProvider(ILogger<OAuthService> logger, IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, string providerName, [FromQuery] string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return Results.BadRequest("No provider name has been specified.");
        }

        logger.LogDebug($"[oauth/register] : provider = '{providerName}'");

        var data = new Dictionary<string, string>();

        var hasRedirectUri = !string.IsNullOrWhiteSpace(redirectUri);

        if (hasRedirectUri)
        {
            data.Add("RedirectUri", redirectUri!);
        }

        var grant = await oauthService.RegisterAsync(providerName, data);

        logger.LogDebug($"[oauth/registered] : grant id = '{grant.Id}' / provider = '{providerName}'");

        var oauthOptionsValue = Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);
        var oauthProviderOptions = oauthOptionsValue.GetProviderOptions(providerName);

        var authorizationUrl = new StringBuilder(oauthProviderOptions.Authorize.Url);

        authorizationUrl.Append($"?response_type=code&client_id={oauthProviderOptions.ClientId}");
        authorizationUrl.Append($"&scope={oauthProviderOptions.Scope}");
        authorizationUrl.Append($"&redirect_uri={(hasRedirectUri ? redirectUri : oauthOptionsValue.DefaultRedirectUri)}");

        if (!string.IsNullOrWhiteSpace(oauthProviderOptions.Authorize.CodeChallengeMethod))
        {
            if (string.IsNullOrWhiteSpace(grant.CodeChallenge))
            {
                return Results.BadRequest($"No 'CodeChallenge' has been generated for code challenge method '{oauthProviderOptions.Authorize.CodeChallengeMethod}'.");
            }

            logger.LogDebug($"[oauth/code challenge] : grant id = '{grant.Id}' / provider = '{providerName}'");

            authorizationUrl.Append($"&code_challenge_method={oauthProviderOptions.Authorize.CodeChallengeMethod}&code_challenge={grant.CodeChallenge}");
        }

        return Results.Ok(new { AuthorizationUrl = authorizationUrl + $"&state={grant.Id}" });
    }

    private static IResult GetProviders(IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, string group = "default")
    {
        Guard.AgainstNull(Guard.AgainstNull(apiOptions).Value);
        Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);

        var result = new List<OAuthProvider>();

        foreach (var oauthProviderOptions in oauthOptions.Value.Providers.Where(item => item.Groups.Count == 0 || item.Groups.Contains(group)))
        {
            var oauthProvider = new OAuthProvider { Name = oauthProviderOptions.Name };

            var path = Path.Combine(apiOptions.Value.ExtensionFolder, "OAuth", $"{oauthProviderOptions.Name}.svg");

            if (File.Exists(path))
            {
                oauthProvider.Svg = File.ReadAllText(path);
            }

            result.Add(oauthProvider);
        }

        return Results.Ok(result);
    }
}