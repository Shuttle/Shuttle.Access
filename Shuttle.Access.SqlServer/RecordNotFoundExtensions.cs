namespace Shuttle.Access.SqlServer;

public static class RecordNotFoundExtensions
{
    extension<T>(T? entity) where T : class
    {
        public T GuardAgainstRecordNotFound(object id)
        {
            return entity ?? throw RecordNotFoundException.For<T>(id);
        }
    }
}