﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc
{
    public class RequiresSessionAttribute : TypeFilterAttribute
    {
        public RequiresSessionAttribute() : base(typeof(RequiresSession))
        {
            Arguments = new object[] { };
        }

        private class RequiresSession : IAuthorizationFilter
        {
            private readonly IAccessService _accessService;

            public RequiresSession(IAccessService accessService)
            {
                Guard.AgainstNull(accessService, nameof(accessService));

                _accessService = accessService;
            }

            private static void SetUnauthorized(AuthorizationFilterContext context)
            {
                context.Result = new UnauthorizedResult();
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var sessionTokenResult = context.HttpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    SetUnauthorized(context);
                    return;
                }

                if (!_accessService.Contains(sessionTokenResult.SessionToken))
                {
                    SetUnauthorized(context);
                }
            }
        }
    }
}