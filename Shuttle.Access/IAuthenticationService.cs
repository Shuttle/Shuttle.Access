namespace Shuttle.Access;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string identityName, string password, CancellationToken cancellationToken = default);
}