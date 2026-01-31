namespace Shuttle.Access;

public class NullSessionCache : ISessionCache
{
    public Messages.v1.Session? Find(Messages.v1.Session.Specification specification)
    {
        return null;
    }

    public Messages.v1.Session Add(Guid? token, Messages.v1.Session session)
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