using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Mvc
{
    public class RequiresSessionAttribute : ActionFilterAttribute
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISessionQuery _sessionQuery;

        public RequiresSessionAttribute(IAccessConfiguration configuration,
            IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(sessionQuery, nameof(sessionQuery));

            _configuration = configuration;
            _databaseContextFactory = databaseContextFactory;
            _sessionQuery = sessionQuery;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            base.OnActionExecuting(actionContext);

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

            using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
            {
                if (!_sessionQuery.Contains(sessionToken))
                {
                    SetUnauthorized(actionContext);
                }
            }
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