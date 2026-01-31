namespace Shuttle.Access;

public interface ISessionCache
{
    Messages.v1.Session? Find(Messages.v1.Session.Specification specification);
    Messages.v1.Session Add(Guid? token, Messages.v1.Session session);
    void Flush();
    void Flush(Guid identityId);
}