using System;

namespace Shuttle.Access.WebApi
{
    public class RegisterPasswordExpiryModel
    {
        public Guid UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool NeverExpires { get; set; }
    }
}