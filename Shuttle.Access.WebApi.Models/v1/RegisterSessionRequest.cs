namespace Shuttle.Access.WebApi.Models.v1
{
    public class RegisterSessionRequest
    {
        public string IdentityName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }
}