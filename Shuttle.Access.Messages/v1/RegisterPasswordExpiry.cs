using System;

namespace Shuttle.Access.Messages.v1
{
    public class RegisterPasswordExpiry
    {
        public Guid IdentityId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool NeverExpires { get; set; }
    }
}