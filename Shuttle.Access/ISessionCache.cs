namespace Shuttle.Access;

public interface ISessionCache
{
    Query.Session? Find(Query.Session.Specification specification);
    Query.Session Add(Query.Session session);
    void Flush();
    void Flush(Guid identityId);
}