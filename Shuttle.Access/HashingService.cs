using System.Security.Cryptography;
using System.Text;

namespace Shuttle.Access;

public class HashingService : IHashingService
{
    public byte[] Sha256(string value)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}