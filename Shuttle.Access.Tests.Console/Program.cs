using System;
using Shuttle.Access.Application;

namespace Shuttle.Access.Tests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new AccessClient(new AccessClientConfiguration("http://localhost:5599/", "access://system", "Vgy)Hbt5"));

            var registerSessionResult = client.RegisterSession("access://test-user");

            if (registerSessionResult.Ok)
            {
                var session = client.GetSession(registerSessionResult.Token);
            }

            client.Logout();
        }
    }
}
