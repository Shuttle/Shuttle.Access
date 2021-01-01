namespace Shuttle.Access.Messages.v1
{
    public class RegisterUserCommand
    {
		public string Username { get; set; }
	    public byte[] PasswordHash { get; set; }
	    public string RegisteredBy { get; set; }
        public string GeneratedPassword { get; set; }
        public bool Activated { get; set; }
    }
}
