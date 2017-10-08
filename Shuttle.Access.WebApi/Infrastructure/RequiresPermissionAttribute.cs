using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Castle.Windsor;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.WebApi
{
    public class RequiresPermissionAttribute : ActionFilterAttribute
    {
        public string Permission { get; private set; }

        public RequiresPermissionAttribute(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, "permission");

            Permission = permission;
        }

        private static IDatabaseContextFactory _databaseContextFactory;
        private static ISessionQuery _sessionQuery;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var headers = actionContext.Request.Headers;
            var sessionTokenValue = GetHeaderValue(headers, "sentinel-sessiontoken");

            if (string.IsNullOrEmpty(sessionTokenValue))
            {
                SetUnauthorized(actionContext);
                return;
            }

            Guid sessionToken;

            if (!Guid.TryParse(sessionTokenValue, out sessionToken))
            {
                SetUnauthorized(actionContext);
                return;
            }

            using (GuardedDatabaseConnectionFactory().Create())
            {
                if (!(GuardedQuery().Contains(sessionToken, Permission) || GuardedQuery().Contains(sessionToken, "*")))
                {
                    SetUnauthorized(actionContext);
                }
            }
        }

        private static ISessionQuery GuardedQuery()
        {
            Guard.AgainstNull(_sessionQuery, "_sessionQuery");

            return _sessionQuery;
        }

        private static IDatabaseContextFactory GuardedDatabaseConnectionFactory()
        {
            Guard.AgainstNull(_databaseContextFactory, "_databaseContextFactory");

            return _databaseContextFactory;
        }

        private static void SetUnauthorized(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        private static string GetHeaderValue(HttpHeaders headers, string name)
        {
            if (!headers.Contains(name))
            {
                return null;
            }

            var tokens = headers.GetValues(name).ToList();

            return tokens.Count != 1 ? null : tokens[0];
        }

        public static void Assign(IWindsorContainer container)
        {
            Guard.AgainstNull(container, "container");

            _databaseContextFactory = container.Resolve<IDatabaseContextFactory>();
            _sessionQuery = container.Resolve<ISessionQuery>();
        }
    }
}