namespace Shuttle.Access.WebApi
{
    public class RegisterUserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Activated { get; set; }
    }
}