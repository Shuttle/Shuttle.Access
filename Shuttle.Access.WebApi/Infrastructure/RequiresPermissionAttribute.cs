using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Access.Sql;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi
{
    public class RequiresPermissionAttribute : TypeFilterAttribute
    {
        public RequiresPermissionAttribute(string permission) : base(typeof(RequiresPermission))
        {
            Arguments = new object[] {permission};
        }

        private class RequiresPermission : IActionFilter
        {
            private readonly IDatabaseContextFactory _databaseContextFactory;
            private readonly string _permission;
            private readonly ISessionQuery _sessionQuery;

            public RequiresPermission(IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery,
                string permission)
            {
                Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
                Guard.AgainstNull(sessionQuery, nameof(sessionQuery));
                Guard.AgainstNullOrEmptyString(permission, "permission");

                _databaseContextFactory = databaseContextFactory;
                _sessionQuery = sessionQuery;
                _permission = permission;
            }

            public void OnActionExecuting(ActionExecutingContext actionContext)
            {
                var headers = actionContext.HttpContext.Request.Headers;
                var sessionTokenValue = GetHeaderValue(headers, "access-sessiontoken");

                if (string.IsNullOrEmpty(sessionTokenValue))
                {
                    SetUnauthorized(actionContext);
                    return;
                }

                if (!Guid.TryParse(sessionTokenValue, out var sessionToken))
                {
                    SetUnauthorized(actionContext);
                    return;
                }

                using (_databaseContextFactory.Create())
                {
                    if (!_sessionQuery.Contains(sessionToken, _permission) || _sessionQuery.Contains(sessionToken, "*"))
                    {
                        SetUnauthorized(actionContext);
                    }
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            private static void SetUnauthorized(ActionContext actionContext)
            {
                actionContext.HttpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            }

            private static string GetHeaderValue(IHeaderDictionary headers, string name)
            {
                if (!headers.ContainsKey(name))
                {
                    return null;
                }

                var tokens = headers[name].ToList();

                return tokens.Count != 1 ? null : tokens[0];
            }
        }
    }
}