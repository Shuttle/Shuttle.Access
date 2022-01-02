namespace Shuttle.Access.Messages.v1
{
    public class RegisterIdentity
    {
		public string Name { get; set; }
        public string Password { get; set; }
	    public byte[] PasswordHash { get; set; }
	    public string RegisteredBy { get; set; }
	    public string GeneratedPassword { get; set; }
        public string System { get; set; }
        public bool Activated { get; set; }
    }
}
