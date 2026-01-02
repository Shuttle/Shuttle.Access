using System.Data;

namespace Shuttle.Access.Data;

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