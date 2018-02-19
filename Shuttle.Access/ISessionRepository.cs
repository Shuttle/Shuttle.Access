using System;

namespace Shuttle.Access
{
	public interface ISessionRepository
	{
		void Save(Session session);
		Session Get(Guid token);
		void Remove(Guid token);
	    void Renewed(Session session);
	}
}