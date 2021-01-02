using System;

namespace Shuttle.Access.WebApi
{
    public class RegisterPasswordExpiryModel
    {
        public Guid IdentityId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool NeverExpires { get; set; }
    }
}