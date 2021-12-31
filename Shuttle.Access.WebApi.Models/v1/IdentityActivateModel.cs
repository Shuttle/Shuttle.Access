using System;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class IdentityActivateModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public DateTime DateActivated { get; set; }
    }
}