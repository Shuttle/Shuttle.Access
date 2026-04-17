namespace Shuttle.Access.SqlServer;

public class RecordNotFoundException(string message) : Exception(message)
{
    public static RecordNotFoundException For(string name, object id)
    {
        return new($"Could not find a record for '{name}' with id '{id}'");
    }

    public static RecordNotFoundException For<T>(object id)
    {
        return For(typeof(T).Name, id);
    }
}