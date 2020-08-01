using System.Security.Cryptography;
using System.Text;

namespace Shuttle.Access
{
    public class HashingService : IHashingService
    {
        public byte[] Sha256(string password)
        {
            return new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password), 0,
                Encoding.UTF8.GetByteCount(password));
        }
    }
}