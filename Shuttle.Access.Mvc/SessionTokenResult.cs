using System;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public class SessionTokenResult
    {
        private SessionTokenResult()
        {
            SessionToken = Guid.Empty;
            ActionResult = null;
        }

        public Guid SessionToken { get; private set; }
        public IActionResult ActionResult { get; private set; }

        public bool Ok => ActionResult == null;

        public static SessionTokenResult Success(Guid sessionToken)
        {
            return new SessionTokenResult
            {
                SessionToken = sessionToken
            };
        }

        public static SessionTokenResult Failure(IActionResult httpActionResult)
        {
            Guard.AgainstNull(httpActionResult, nameof(httpActionResult));

            return new SessionTokenResult
            {
                ActionResult = httpActionResult
            };
        }
    }
}