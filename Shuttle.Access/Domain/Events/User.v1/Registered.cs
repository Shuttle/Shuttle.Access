using System;

namespace Shuttle.Sentinel.DomainEvents.User.v1
{
	public class Registered
	{
		public string Username { get; set; }
		public byte[] PasswordHash { get; set; }
		public string RegisteredBy { get; set; }
		public DateTime DateRegistered { get; set; }
	}
}