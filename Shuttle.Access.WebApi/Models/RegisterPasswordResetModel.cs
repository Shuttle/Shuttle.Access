using System;

namespace Shuttle.Access.WebApi
{
    public class RegisterPasswordResetModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}