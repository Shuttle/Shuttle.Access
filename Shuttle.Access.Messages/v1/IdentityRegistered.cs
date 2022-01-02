namespace Shuttle.Access.Messages.v1
{
    public class IdentityRegistered
    {
		public string Name { get; set; }
	    public string RegisteredBy { get; set; }
        public string GeneratedPassword { get; set; }
        public bool Activated { get; set; }
        public string System { get; set; }
    }
}
