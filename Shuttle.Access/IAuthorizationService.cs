using System.Collections.Generic;

namespace Shuttle.Access
{
	public interface IAuthorizationService
	{
		IEnumerable<string> Permissions(string username, object authenticationTag);
	}
}