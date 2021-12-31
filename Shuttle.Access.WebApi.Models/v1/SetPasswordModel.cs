namespace Shuttle.Access.WebApi.Models.v1
{
    public class SetPasswordModel
    {
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}