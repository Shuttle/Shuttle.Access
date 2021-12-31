using System;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class RegisterPasswordResetModel
    {
        public Guid IdentityId { get; set; }
        public string Token { get; set; }
    }
}