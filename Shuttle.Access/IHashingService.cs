namespace Shuttle.Access
{
    public interface IHashingService
    {
        byte[] Sha256(string password);
    }
}