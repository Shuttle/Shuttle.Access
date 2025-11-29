using Microsoft.IdentityModel.JsonWebTokens;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class JwtIssuerOptionsUnavailableEventArgs(JsonWebToken jsonWebToken)
{
    public JsonWebToken JsonWebToken { get; } = Guard.AgainstNull(jsonWebToken);
}