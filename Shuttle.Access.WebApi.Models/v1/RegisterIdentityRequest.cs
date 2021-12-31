namespace Shuttle.Access.WebApi.Models.v1
{
    public class RegisterIdentityRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Activated { get; set; }
        public string System { get; set; }
    }
}