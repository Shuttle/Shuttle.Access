using System;

namespace Shuttle.Access.RestClient
{
    public class AccessRestClientOptions
    {
        public Uri BaseAddress { get; set; }
        public string IdentityName { get; set; }
        public string Password { get; set; }
    }
}