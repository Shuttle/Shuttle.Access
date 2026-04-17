namespace Shuttle.Access;

public class NullSessionCache : ISessionCache
{
    public Query.Session? Find(Query.Session.Specification specification)
    {
        return null;
    }

    public Query.Session Add(Query.Session session)
    {
        return session;
    }

    public void Flush()
    {
    }

    public void Flush(Guid identityId)
    {
    }
}