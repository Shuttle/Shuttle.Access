using System;
using System.Collections.Generic;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class RegisterSessionResponse
    {
        public bool Success { get; set; }
        public string IdentityName { get; set; }
        public Guid? Token { get; set; }
        public DateTime? TokenExpiryDate { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }
}