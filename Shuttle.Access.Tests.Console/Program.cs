using System;
using Shuttle.Access.Api;

namespace Shuttle.Access.Tests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new AccessClient(new ClientConfiguration("http://localhost:5599/", "system://access", "Vgy)Hbt5"));
            
            client.Logout();
        }
    }
}
