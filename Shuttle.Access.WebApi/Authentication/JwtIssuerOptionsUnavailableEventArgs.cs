using Microsoft.IdentityModel.JsonWebTokens;
using Shuttle.Contract;

namespace Shuttle.Access.WebApi;

public class JwtIssuerOptionsUnavailableEventArgs(JsonWebToken jsonWebToken)
{
    public JsonWebToken JsonWebToken { get; } = Guard.AgainstNull(jsonWebToken);
}
