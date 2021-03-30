using System;
using Shuttle.Access.Api;

namespace Shuttle.Access.Tests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new AccessClient(new ClientConfiguration("http://localhost:5599/", "system://access", "Vgy)Hbt5"));
            
            client.Activate("fidelior://user/1962bda6-30f4-4e70-825a-cfdc3d833eb1", DateTime.Now);

            var passwordResetToken = client.GetPasswordResetToken("fidelior://user/1962bda6-30f4-4e70-825a-cfdc3d833eb1");
            
            client.ResetPassword("fidelior://user/1962bda6-30f4-4e70-825a-cfdc3d833eb1", passwordResetToken, "new-password");
            
            client.Logout();
        }
    }
}
