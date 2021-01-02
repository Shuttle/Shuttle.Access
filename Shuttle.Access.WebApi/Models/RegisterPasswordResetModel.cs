using System;

namespace Shuttle.Access.WebApi
{
    public class RegisterPasswordResetModel
    {
        public Guid IdentityId { get; set; }
        public string Token { get; set; }
    }
}