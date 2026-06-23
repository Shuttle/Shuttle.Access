using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Mediator;
using Shuttle.OAuth;
using RegisterIdentity = Shuttle.Access.Messages.v1.RegisterIdentity;
using SessionRequest = Shuttle.Access.Application.SessionRequest;

namespace Shuttle.Access.WebApi;

public static class OAuthEndpoints
{
    private static async Task<IResult> GetAuthenticateProvider(IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, string provider, string application, [FromQuery] string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Results.BadRequest("No provider name has been specified.");
        }

        var data = new Dictionary<string, string> { { "Application", string.IsNullOrWhiteSpace(application) ? "Access" : application } };

        var hasRedirectUri = !string.IsNullOrWhiteSpace(redirectUri);

        if (hasRedirectUri)
        {
            data.Add("RedirectUri", redirectUri!);
        }

        var grant = await oauthService.RegisterAsync(provider, data);

        var oauthOptionsValue = Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);
        var oauthProviderOptions = oauthOptionsValue.GetProviderOptions(provider);

        var authorizationUrl = new StringBuilder(oauthProviderOptions.Authorize.Url);

        authorizationUrl.Append($"?response_type=code&client_id={oauthProviderOptions.ClientId}");
        authorizationUrl.Append($"&scope={oauthProviderOptions.Scope}");
        authorizationUrl.Append($"&redirect_uri={(hasRedirectUri ? redirectUri : oauthProviderOptions.RedirectUri)}");

        if (!string.IsNullOrWhiteSpace(oauthProviderOptions.Authorize.CodeChallengeMethod))
        {
            if (string.IsNullOrWhiteSpace(grant.CodeChallenge))
            {
                return Results.BadRequest($"No 'CodeChallenge' has been generated for code challenge method '{oauthProviderOptions.Authorize.CodeChallengeMethod}'.");
            }

            authorizationUrl.Append($"&code_challenge_method={oauthProviderOptions.Authorize.CodeChallengeMethod}&code_challenge={grant.CodeChallenge}");
        }

        return Results.Ok(new { AuthorizationUrl = authorizationUrl + $"&state={grant.Id}" });
    }

    private static async Task<IResult> GetIdentityStateCode(IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, string state, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || !Guid.TryParse(state, out var grantId))
        {
            return Results.BadRequest();
        }

        var grant = await oauthGrantRepository.GetAsync(grantId, cancellationToken);
        var data = await oauthService.GetDataAsync(grant, code, cancellationToken);
        var oauthProviderOptions = Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value).GetProviderOptions(grant.ProviderName);

        if (string.IsNullOrWhiteSpace(oauthProviderOptions.Data.IdentityPropertyName))
        {
            return Results.Problem($"The 'Data.IdentityPropertyName' is empty for the '{grant.ProviderName}' provider options.");
        }

        var identityName = data.GetProperty(oauthProviderOptions.Data.IdentityPropertyName).ToString();

        return string.IsNullOrWhiteSpace(identityName)
            ? Results.BadRequest($"No identity property '{oauthProviderOptions.Data.IdentityPropertyName}' was returned from the data endpoint provider.")
            : Results.Ok(new OAuthIdentity { IdentityName = identityName });
    }

    private static IResult GetProviders(IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, string application = "")
    {
        Guard.AgainstNull(Guard.AgainstNull(apiOptions).Value);
        Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);

        var result = new List<OAuthProvider>();

        foreach (var pair in oauthOptions.Value.Providers.Where(item => string.IsNullOrWhiteSpace(application) || item.Value.Applications.Count == 0 || item.Value.Applications.Any(app => app.Equals(application, StringComparison.CurrentCultureIgnoreCase))))
        {
            var providerOptions = pair.Value;

            var oauthProvider = new OAuthProvider
            {
                Name = pair.Key,
                DisplayName = string.IsNullOrWhiteSpace(providerOptions.DisplayName) ? pair.Key : providerOptions.DisplayName
            };

            var fileName = string.IsNullOrWhiteSpace(providerOptions.SvgFileName)
                ? pair.Key
                : providerOptions.SvgFileName;

            var path = Path.ChangeExtension(Path.Combine(apiOptions.Value.ExtensionFolder, "OAuth", fileName), ".svg");

            if (File.Exists(path))
            {
                oauthProvider.Svg = File.ReadAllText(path);
            }

            result.Add(oauthProvider);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSessionStateCode(IOptions<AccessOptions> accessOptions, IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IBus bus, IMediator mediator, string state, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || !Guid.TryParse(state, out var grantId))
        {
            return Results.BadRequest();
        }

        var grant = await oauthGrantRepository.GetAsync(grantId, cancellationToken);
        var data = await oauthService.GetDataAsync(grant, code, cancellationToken);
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

        if (!grant.Data.TryGetValue("Application", out var application))
        {
            application = "Access";
        }

        var registerSession = new SessionRequest(identityName).WithApplication(application).UseDirect();

        await mediator.SendAsync(registerSession, cancellationToken);

        var requestRegistration = registerSession.Result == SessionRequestResult.UnknownIdentity && apiOptions.Value.OAuthRegisterUnknownIdentities;

        if (requestRegistration)
        {
            await bus.SendAsync(new RegisterIdentity
            {
                Id = Guid.NewGuid(),
                Name = identityName,
                RegisteredBy = grant.ProviderName,
                AuditTenantId = accessOptions.Value.SystemTenantId,
                AuditIdentityName = "system",
                Activated = true
            }, cancellationToken);
        }

        return Results.Ok(registerSession.GetSessionResponse(requestRegistration));
    }

    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/providers/{application}", GetProviders)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{provider}/{application?}", GetAuthenticateProvider)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{*code}", GetSessionStateCode)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/identity/{state}/{*code}", GetIdentityStateCode)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}