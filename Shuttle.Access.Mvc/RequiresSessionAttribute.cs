using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

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
            private readonly IAccessConfiguration _configuration;
            private readonly IDatabaseContextFactory _databaseContextFactory;
            private readonly ISessionQuery _sessionQuery;

            public RequiresSession(IAccessConfiguration configuration,
                IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery)
            {
                Guard.AgainstNull(configuration, nameof(configuration));
                Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
                Guard.AgainstNull(sessionQuery, nameof(sessionQuery));

                _configuration = configuration;
                _databaseContextFactory = databaseContextFactory;
                _sessionQuery = sessionQuery;
            }

            private static void SetUnauthorized(AuthorizationFilterContext context)
            {
                context.Result = new UnauthorizedResult();
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

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var headers = context.HttpContext.Request.Headers;
                var sessionTokenValue = GetHeaderValue(headers, "access-sessiontoken");

                if (string.IsNullOrEmpty(sessionTokenValue))
                {
                    SetUnauthorized(context);
                    return;
                }

                if (!Guid.TryParse(sessionTokenValue, out var sessionToken))
                {
                    SetUnauthorized(context);
                    return;
                }

                using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
                {
                    if (!_sessionQuery.Contains(sessionToken))
                    {
                        SetUnauthorized(context);
                    }
                }
            }
        }
    }
}