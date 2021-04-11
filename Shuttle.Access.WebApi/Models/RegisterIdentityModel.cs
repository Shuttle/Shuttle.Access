namespace Shuttle.Access.WebApi
{
    public class RegisterIdentityModel
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Activated { get; set; }
        public string System { get; set; }
    }
}