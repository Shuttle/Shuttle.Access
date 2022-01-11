using System.Security.Cryptography;
using System.Text;

namespace Shuttle.Access
{
    public class HashingService : IHashingService
    {
        public byte[] Sha256(string value)
        {
            return new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(value), 0, Encoding.UTF8.GetByteCount(value));
        }
    }
}