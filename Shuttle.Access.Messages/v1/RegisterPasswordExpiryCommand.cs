using System;

namespace Shuttle.Access.Messages.v1
{
    public class RegisterPasswordExpiryCommand
    {
        public Guid UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool NeverExpires { get; set; }
    }
}