namespace Shuttle.Access
{
	public interface IAuthenticationService
	{
		AuthenticationResult Authenticate(string username, string password);
	}
}