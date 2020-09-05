using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Access.DataAccess;
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

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var sessionTokenValue = context.HttpContext.GetAccessSessionToken();

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