using System;

namespace Shuttle.Access
{
    public class AuthenticationEventArgs : EventArgs
    {
        public bool Authenticated { get; }
        public string Message { get; }

        public AuthenticationEventArgs()
        {
        }

        public AuthenticationEventArgs(bool authenticated, string message)
        {
            Authenticated = authenticated;
            Message = message ?? string.Empty;
        }
    }
}