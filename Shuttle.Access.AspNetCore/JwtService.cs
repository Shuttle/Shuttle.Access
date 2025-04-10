using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class JwtService : IJwtService
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions());
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonWebTokenHandler _jwtHandler = new();
    private readonly AccessAuthorizationOptions _accessAuthorizationOptionsOptions;

    public JwtService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IHttpClientFactory httpClientFactory)
    {
        _accessAuthorizationOptionsOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
        _httpClientFactory = Guard.AgainstNull(httpClientFactory);
    }

    public async ValueTask<string> GetIdentityNameAsync(string token)
    {
        var jsonToken = _jwtHandler.ReadJsonWebToken(Guard.AgainstEmpty(token));
        var options = GetOptions(jsonToken);

        if (options == null)
        {
            return string.Empty;
        }

        Claim? claim = null;

        foreach (var identityNameClaimType in options.IdentityNameClaimTypes)
        {
            claim = jsonToken.Claims.FirstOrDefault(item => item.Type.Equals(identityNameClaimType, StringComparison.InvariantCultureIgnoreCase));

            if (claim != null)
            {
                break;
            }
        }

        return await ValueTask.FromResult(claim?.Value ?? string.Empty);
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        var jwt = _jwtHandler.ReadJsonWebToken(Guard.AgainstEmpty(token));
        var options = GetOptions(jwt);

        if (options == null)
        {
            return new()
            {
                Exception = new InvalidOperationException(string.Format(Resources.IssuerNotFoundException, jwt.Issuer, string.Join(',', jwt.Audiences ?? [])))
            };
        }

        var keys = await GetSigningKeysAsync(options);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Uri,
            ValidateAudience = options.Audiences.Any(),
            ValidAudiences = options.Audiences,
            ValidateLifetime = true,
            ClockSkew = options.ClockSkew,
            IssuerSigningKeys = keys
        };

        return await _jwtHandler.ValidateTokenAsync(token, validationParameters);
    }

    private IssuerOptions? GetOptions(JsonWebToken jwt)
    {
        return _accessAuthorizationOptionsOptions.Issuers.FirstOrDefault(item =>
            item.Uri.Equals(jwt.Issuer, StringComparison.CurrentCultureIgnoreCase) &&
            (
                !item.Audiences.Any() ||
                item.Audiences.Intersect(jwt.Audiences).Any()
            ));
    }

    private async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync(IssuerOptions options)
    {
        if (Cache.TryGetValue(options.JwksUri, out IEnumerable<SecurityKey>? cachedKeys))
        {
            return cachedKeys!;
        }

        var httpClient = _httpClientFactory.CreateClient("Shuttle.OAuth");
        var response = await httpClient.GetAsync(options.JwksUri);

        var cacheControlHeader = response.Headers.CacheControl;
        var cacheDuration = cacheControlHeader?.MaxAge.HasValue == true
            ? cacheControlHeader.MaxAge.Value
            : options.SigningKeyCacheDuration;

        var jwksContent = await response.Content.ReadAsStringAsync();
        var jwks = JsonDocument.Parse(jwksContent);
        var keys = new List<SecurityKey>();

        foreach (var key in jwks.RootElement.GetProperty("keys").EnumerateArray())
        {
            var e = Base64UrlEncoder.DecodeBytes(key.GetProperty("e").GetString());
            var n = Base64UrlEncoder.DecodeBytes(key.GetProperty("n").GetString());
            var rsa = new RSAParameters { Exponent = e, Modulus = n };
            keys.Add(new RsaSecurityKey(rsa));
        }

        Cache.Set(options.JwksUri, keys, cacheDuration);

        return keys;
    }
}