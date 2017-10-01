using System.Collections.Generic;

namespace Shuttle.Access
{
	public interface IAnonymousPermissions
	{
		IEnumerable<string> AnonymousPermissions();
	}
}