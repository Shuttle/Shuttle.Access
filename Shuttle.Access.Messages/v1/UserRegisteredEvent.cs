namespace Shuttle.Access.Messages.v1
{
    public class UserRegisteredEvent
    {
		public string Username { get; set; }
	    public string RegisteredBy { get; set; }
    }
}
