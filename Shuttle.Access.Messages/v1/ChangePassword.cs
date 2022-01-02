namespace Shuttle.Access.Messages.v1
{
    public class ChangePassword
    {
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}