using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.OAuth;
using RegisterIdentity = Shuttle.Access.Messages.v1.RegisterIdentity;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class OAuthEndpoints
{
    public static WebApplication MapOAuthEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/oauth/providers/{group}", GetProviders)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/authenticate/{provider}", GetAuthenticateProvider)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/oauth/session/{state}/{code}", GetSessionStateCode)
            .WithTags("OAuth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> GetSessionStateCode(IOptions<AccessOptions> accessOptions, IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, IOAuthGrantRepository oauthGrantRepository, IBus bus, IMediator mediator, string state, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || !Guid.TryParse(state, out var grantId))
        {
            return Results.BadRequest();
        }

        var grant = await oauthGrantRepository.GetAsync(grantId);

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

        var registerSession = new RegisterSession(identityName).Refresh().UseDirect();

        await mediator.SendAsync(registerSession, cancellationToken);

        foreach (var session in registerSession.SessionsRemoved)
        {
            await bus.PublishAsync(new SessionDeleted
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName,
                TenantId = session.TenantId
            }, cancellationToken);
        }

        var requestRegistration = registerSession.Result == SessionRegistrationResult.UnknownIdentity && apiOptions.Value.OAuthRegisterUnknownIdentities;

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

    private static async Task<IResult> GetAuthenticateProvider(IOptions<AccessOptions> accessOptions, IOptions<OAuthOptions> oauthOptions, IOAuthService oauthService, string provider, [FromQuery] string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Results.BadRequest("No provider name has been specified.");
        }

        var data = new Dictionary<string, string>();

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

    private static IResult GetProviders(IOptions<ApiOptions> apiOptions, IOptions<OAuthOptions> oauthOptions, string groupName = "")
    {
        Guard.AgainstNull(Guard.AgainstNull(apiOptions).Value);
        Guard.AgainstNull(Guard.AgainstNull(oauthOptions).Value);

        var result = new List<OAuthProvider>();

        foreach (var pair in oauthOptions.Value.Providers.Where(item => string.IsNullOrWhiteSpace(groupName) || item.Value.Groups.Count == 0 || item.Value.Groups.Any(group=>  group.Equals(groupName,StringComparison.CurrentCultureIgnoreCase))))
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
}