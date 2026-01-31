using Microsoft.IdentityModel.Tokens;

namespace Shuttle.Access.AspNetCore;

public interface IJwtService
{
    ValueTask<string> GetIdentityNameAsync(string token);
    Task<TokenValidationResult> ValidateTokenAsync(string token);
}