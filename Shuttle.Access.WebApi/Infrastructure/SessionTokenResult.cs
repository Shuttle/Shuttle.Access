using System;
using System.Web.Http;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.WebApi
{
    public class SessionTokenResult
    {
        private SessionTokenResult()
        {
            SessionToken = Guid.Empty;
            HttpActionResult = null;
        }

        public Guid SessionToken { get; private set; }
        public IHttpActionResult HttpActionResult { get; private set; }

        public bool OK
        {
            get { return HttpActionResult == null; }
        }

        public static SessionTokenResult Success(Guid sessionToken)
        {
            return new SessionTokenResult
            {
                SessionToken = sessionToken
            };
        }

        public static SessionTokenResult Failure(IHttpActionResult httpActionResult)
        {
            Guard.AgainstNull(httpActionResult, "httpActionResult");

            return new SessionTokenResult
            {
                HttpActionResult = httpActionResult
            };
        }
    }
}